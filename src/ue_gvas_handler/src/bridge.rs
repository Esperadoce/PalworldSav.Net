extern crate libc;
extern crate uesave;

use std::io::Cursor;
use uesave::{Save, StructType, Types};
use libc::{c_char, c_void};
use std::ffi::{CStr, CString};

#[repr(C)]
pub struct KeyValuePair {
    key: *const c_char,
    value: *const c_char,
}

#[repr(C)]
pub struct Result {
    ptr: *const c_void,
    size: usize,
}

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

#[no_mangle]
pub extern "C" fn serialize(json: *const c_char,  size: *mut usize) -> *mut c_void {
    let json = unsafe { CStr::from_ptr(json).to_string_lossy().into_owned() };
    let decoded: Save = serde_json::from_str(&json).unwrap();
    let mut writer: Vec<u8> = Vec::new();
    let _ = decoded.write(&mut writer).unwrap();

    // Convert the Vec<u8> to a raw pointer
    let ptr = writer.as_ptr();
    let len = writer.len();
    std::mem::forget(writer);

    // Set the size
    unsafe { *size = len; }

    ptr as *mut c_void
}

#[no_mangle]
pub extern "C" fn free_rust_string(s: *mut c_char) {
    unsafe {
        if s.is_null() { return }
        let _ = CString::from_raw(s);
    };
}

#[no_mangle]
pub extern "C" fn free_rust_vec(p: *mut c_void) {
    unsafe {
        if p.is_null() { return }
        Vec::from_raw_parts(p, 0, 1);
    };
}
