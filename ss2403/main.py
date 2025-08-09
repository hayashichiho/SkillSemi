from gui_app import GuiApplication
import tkinter as tk

def main():
    root = tk.Tk()
    app = GuiApplication(master=root)
    app.mainloop()

if __name__ == "__main__":
    main()
