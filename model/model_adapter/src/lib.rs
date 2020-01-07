use differential_datalog::DDlog;
use differential_datalog::program::Update;
use differential_datalog::record::*;
use model_ddlog::api::*;
use model_ddlog::*;

pub struct Model {
    prog: HDDlog,
}

impl Model {
    fn new() -> Result<Self, String> {
        fn no_op(_table: usize, _rec: &Record, _w: isize) {}
        let prog = HDDlog::run(1, true, no_op)?;
        Ok(Model {prog})
    }

    fn Comprehension(&self, ctx: u32, ty: u32) -> u32 {
        let mut updates = Vec::new();
        updates.push(Update::Insert {

        })
        self.prog.transaction_start();
        self.prog.apply_valupdates()
        self.prog.apply_updates()
    }
}

#[no_mangle]
pub unsafe extern "C" fn model_create() -> *mut Model {
    let b = Box::new(Model::new().unwrap());
    Box::into_raw(b)
}

#[no_mangle]
pub unsafe extern "C" fn model_destroy(prog: *mut Model) -> () {
    drop(Box::from_raw(prog))
}