#include <cassert>
#include <closure.hpp>
#include <cwf.hpp>
#include <partial_structure.hpp>
#include <phl.hpp>
#include <util.hpp>

extern "C" partial_structure* create_cwf() {
    return new partial_structure(cwf::cwf_signature);
}

extern "C" void destroy_cwf(partial_structure* pstruct) {
    delete pstruct;
}

extern "C" const sort* get_sort(const char* name) {
    return &*std::find_if(
        cwf::cwf_signature.sorts.begin(),
        cwf::cwf_signature.sorts.end(),
        [&](auto& sort) {
            return sort == name;
        });
}

extern "C" const operation* get_op(const char* name) {
    return &*std::find_if(
        cwf::cwf_signature.operations.begin(),
        cwf::cwf_signature.operations.end(),
        [&](auto& op) { return op.name == name; }
    );
}

extern "C" const predicate* get_predicate(const char* name) {
    return &*std::find_if(
        cwf::cwf_signature.predicates.begin(),
        cwf::cwf_signature.predicates.end(),
        [&](auto& pred) { return pred.name == name; }
    );
}

extern "C" bool are_equal(partial_structure* pstruct, size_t l, size_t r) {
    return get_representative(pstruct->equality, l) ==
           get_representative(pstruct->equality, r);
}

extern "C" size_t define_operation(partial_structure* pstruct, const operation* op, size_t* args) {
    auto& rel = pstruct->relations[*op];
    for (auto& row : rel) {
        if (std::equal(args, args + op->dom.size(), row.begin())) {
            return row[op->dom.size()];
        }
    }

    size_t new_id = pstruct->carrier.size();
    pstruct->carrier[new_id] = op->cod;

    std::vector<size_t> vec;
    vec.reserve(op->dom.size() + 1);
    vec.insert(vec.begin(), args, args + op->dom.size());
    vec.push_back(new_id);
    rel.insert(std::move(vec));
    return new_id;
}

extern "C" void define_predicate(partial_structure* pstruct, const predicate* pred, size_t* args) {
    std::vector<size_t> vec(args, args + pred->arity.size());
    pstruct->relations[*pred].insert(vec);
}

extern "C" void compute_fixpoint(partial_structure* pstruct) {
    surjective_closure(cwf::cwf.surjective_axioms, *pstruct);
}