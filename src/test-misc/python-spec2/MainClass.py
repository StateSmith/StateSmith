from Printer import Printer
from Spec2Sm import Spec2Sm

class MainClass:

    @staticmethod
    def actual_main(args):
        sm = Spec2Sm()

        Printer.print_start()
        sm.start()
        print()

        # args = ["INC", "INC", "OFF"] # when debugging, uncomment this line

        for i, arg in enumerate(args):
            try:
                event_id = Spec2Sm.EventId[arg.upper()]
                Printer.print_dispatch_event_name(arg)
                sm.dispatch_event(event_id)
                print()
            except KeyError:
                raise ValueError(f"bad i:{i} arg: `{arg}`")

    @staticmethod
    def main(args):
        MainClass.actual_main(args)


# Assuming `Spec2Sm` and its `EventId` enumeration are defined elsewhere.
# Example of how to run this:
if __name__ == "__main__":
    import sys
    MainClass.main(sys.argv[1:])
