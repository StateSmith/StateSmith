from lightbulb_callback import LightbulbCallback
from lightbulb import lightbulb

def main():
    callback = LightbulbCallback()
    bulb = lightbulb(callback)
    bulb.start()

    print("Press <enter> to toggle the light switch.")
    print("Press ^C to quit.")

    while True:
        input()
        bulb.dispatchEvent(lightbulb.EventId.SWITCH)


if __name__ == "__main__":
    main()