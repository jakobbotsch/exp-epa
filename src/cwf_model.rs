use super::model::Model;

mod phl {
    use libc::{size_t, c_char};
    use std::ffi::CString;

    extern {
        fn create_cwf() -> size_t;
        fn get_op(name: *const c_char) -> size_t;
    }

    fn gop(name: &str) -> size_t {
        let cstr = CString::new(name).unwrap();
        unsafe { get_op(cstr.as_ptr()) }
    }

    lazy_static! {
        pub static ref DOM: size_t = gop("dom");
    }
}

pub struct CwfModel {
    
}

