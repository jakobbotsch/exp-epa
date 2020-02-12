#[macro_use]
extern crate lalrpop_util;
#[macro_use]
extern crate lazy_static;
extern crate libc;

mod lang;
mod cwf;
mod model;
mod cwf_model;
mod type_checker;

fn main() {
    let negb = "
def negb (b : bool) : bool :=
    elim b into (_ : bool) : bool
    | => false
    | => true
    end.";

    let negb = lang::parser::DefParser::new().parse(negb);
    let result = 
    println!("{:?}", negb.unwrap());
}
