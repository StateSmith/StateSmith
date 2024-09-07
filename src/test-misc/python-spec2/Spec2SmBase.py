from Printer import Printer

class Spec2SmBase:
    @staticmethod
    def trace_guard(message: str, b: bool) -> bool:
        return Printer.trace_guard(message, b)
