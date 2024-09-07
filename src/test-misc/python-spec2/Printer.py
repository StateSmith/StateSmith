class Printer:
    @staticmethod
    def trace(msg: str):
        print(msg)
        print(flush=True)  # Ensures output is flushed immediately

    @staticmethod
    def trace_guard(msg: str, guard: bool) -> bool:
        print(f"{msg} ", end='')

        if guard:
            print("Behavior running.")
        else:
            print("Behavior skipped.")

        return guard

    @staticmethod
    def print_divider():
        print("===================================================")

    @staticmethod
    def print_start():
        print("Start Statemachine")
        Printer.print_divider()

    @staticmethod
    def print_dispatch_event_name(event_name: str):
        print(f"Dispatch event {event_name}")
        Printer.print_divider()
        