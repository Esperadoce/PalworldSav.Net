/*
This Rust library provides a C-compatible Foreign Function Interface (FFI) for handling Unreal Engine GVAS format data.

Features:
- Deserialization: Transforms Unreal Engine GVAS format data into a Json string via FFI.
- Serialization: Converts a Json string back into Unreal Engine GVAS format via FFI.
- Memory Management: Includes FFI functions to deallocate memory for Rust strings and vectors.

Designed specifically for scenarios requiring interaction with Unreal Engine GVAS format data in a C-compatible FFI and Rust mixed environment.
*/


// Importing necessary libraries and modules
extern crate libc;
extern crate uesave;

use std::io::Cursor;
use uesave::{Save, StructType, Types};
use libc::{c_char, c_void};
use std::ffi::{CStr, CString};

// Defining a struct to hold key-value pairs
// This struct is used in the deserialization process
#[repr(C)]
pub struct KeyValuePair {
    key: *const c_char,   // Key in the key-value pair
    value: *const c_char, // Value in the key-value pair
}

// Defining a struct to hold result data
// This struct is used in the serialization process
#[repr(C)]
pub struct Result {
    ptr: *const c_void, // Pointer to the serialized data
    size: usize,        // Size of the serialized data
}


/*
Function to deserialize data from Unreal Engine GVAS format to a Rust structure.
This function takes a buffer, its size, a map, and the map's length as parameters, and returns a pointer to a character. 
The buffer should contain the data in Unreal Engine GVAS format. The map should contain key-value pairs that represent the structure of the data.

The function first converts the raw buffer and map into Rust data structures. 
It then uses these data structures to deserialize the buffer into a Rust structure. The deserialized data is then used by the EA Save library.

The function returns a pointer to a C string that represents the deserialized data. 
This C string can be used in C code and should be deallocated using the `free_rust_string` function when it is no longer needed.
Parameters:
- buffer: A pointer to the data in Unreal Engine GVAS format.
- size: The size of the buffer.
- map: A pointer to an array of KeyValuePair structures that represent the structure of the data.
- map_length: The number of elements in the map.

Returns:
- A pointer to a C string that represents the deserialized data.
*/
#[no_mangle]
pub extern "C" fn deserialize(buffer: *const c_void, size: libc::size_t, map: *const KeyValuePair, map_length: libc::size_t) -> *mut c_char {
    // Convert the raw buffer to a byte slice
    let buffer = unsafe { std::slice::from_raw_parts(buffer as *const u8, size as usize) };

    // Convert the raw map to a slice of KeyValuePair
    let map = unsafe { std::slice::from_raw_parts(map, map_length as usize) };

    let mut types = Types::new();

    for kv in map {
        let key = unsafe { CStr::from_ptr(kv.key).to_string_lossy().into_owned() };
        let value = unsafe { CStr::from_ptr(kv.value).to_string_lossy().into_owned() };
        types.add(key, StructType::Struct(Some(value)));
    }

    let mut file = Cursor::new(buffer);

    let save = Save::read_with_types(&mut file, &types);

    if save.is_err() {
        panic!("{}", save.err().unwrap().to_string());
    }

    let encoded = serde_json::to_string(&save.unwrap());
    if encoded.is_err() {
        panic!("{}", encoded.err().unwrap().to_string());
    }

    // Convert the Rust String to a C string
    let c_str = CString::new(encoded.unwrap()).unwrap();
    c_str.into_raw()
}

/*
Function to serialize data from a json string to Unreal Engine GVAS format.
This function takes a pointer to a JSON string and a pointer to its size as parameters, 
and returns a pointer to void. 
The JSON string should represent a Rust structure that is compatible with the Unreal Engine GVAS format.

The function first converts the JSON string into a Rust `Save` structure using the `serde_json::from_str` function. 
It then serializes this `Save` structure into a byte vector in the Unreal Engine GVAS format.

The function returns a pointer to the serialized data. 
This pointer can be used in C code and should be deallocated using the `free_rust_vec` function when it is no longer needed.

Parameters:
- json: A pointer to a Unreal Engine JSON string that represents a Rust structure.
- size: A pointer to a variable where the size of the serialized data will be stored.
- capacity: A pointer to a variable where the capacity of the serialized data will be stored.

Returns:
- A pointer to the serialized data in the Unreal Engine GVAS format.
*/
#[no_mangle]
pub extern "C" fn serialize(json: *const c_char,  size: *mut usize, cap: *mut usize) -> *mut c_void {
    let json = unsafe { CStr::from_ptr(json).to_string_lossy().into_owned() };
    let decoded: Save = serde_json::from_str(&json).unwrap();
    let mut writer: Vec<u8> = Vec::new();
    let _ = decoded.write(&mut writer).unwrap();

    // Convert the Vec<u8> to a raw pointer
    let ptr = writer.as_ptr();
    let len = writer.len();
    let capacity = writer.capacity();
    std::mem::forget(writer);

    // Set the size
    unsafe { 
        *size = len; 
        *cap = capacity;
    }

    ptr as *mut c_void
}

// Function to free memory allocated for a Rust string
// This function is used to free the memory allocated for a Rust string.
#[no_mangle]
pub extern "C" fn free_rust_string(s: *mut c_char) {
    unsafe {
        if s.is_null() { return }
        let _ = CString::from_raw(s);
    };
}

// Function to free memory allocated for a Rust vector
// This function is used to free the memory allocated for a Rust vector.
#[no_mangle]
pub extern "C" fn free_rust_vec(p: *mut c_void, len: usize, cap: usize) {
    unsafe {
        if p.is_null() { return }
        let _ = Vec::from_raw_parts(p as *mut u8, len, cap);
    };
}