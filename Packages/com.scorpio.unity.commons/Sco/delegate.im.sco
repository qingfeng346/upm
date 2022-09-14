class Delegate {
    constructor() {
        this.addFunc = []
        this.delFunc = []
        this.funcs = []
    }
    clear() {
        this.addFunc = []
        this.delFunc = []
        this.funcs = []
    }
    "+"(func) {
        if (func) {
            this.addFunc.add(func)
        }
        return this
    }
    "-"(func) {
        if (func) {
            this.delFunc.add(func)
        }
        return this
    }
    "()"(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
        if (this.addFunc.length() > 0) {
            this.addFunc.forEach((func) => { this.funcs.add(func) })
            this.addFunc.clear()
        }
        if (this.delFunc.length() > 0) {
            this.delFunc.forEach((func) => { this.funcs.remove(func) })
            this.delFunc.clear()
        }
        foreach (var pair in pairs(this.funcs)) {
            pair.value(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
        }
    }
}