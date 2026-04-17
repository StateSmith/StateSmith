import Printer

class Spec2SmBase
    def trace_guard(message, condition)
        return Printer.trace_guard(message, condition)
    end
end

var Spec2SmBaseModule = module("Spec2SmBase")
Spec2SmBaseModule.Spec2SmBase = Spec2SmBase
return Spec2SmBaseModule
