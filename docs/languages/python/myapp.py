from lightbulb import Lightbulb

def main():
    bulb = Lightbulb()
    bulb.start()

    print("Press <enter> to toggle the light switch.")
    print("Press ^C to quit.")

    while True:
        input()
        bulb.dispatchEvent(Lightbulb.EventId.SWITCH)


if __name__ == "__main__":
    main()