So what is this? This is Twix.

What is Twix? Twix is a feature packed alternative to ntrViewer, kitkat, SnickerStream, and input redirection programs. It's oriented more toward gamers, streamers, and people doing gameplay recordings.

In short, it's a fancy schmancy program that lets you stream Nintendo 3DS games to your computer over wifi. Though you have to modify your Nintendo 3DS in a way that absolutely voids the warranty. Specifically you need to install NTR on your Nintendo 3DS.


http://www.youtube.com/watch?v=0gLN7d8pDV4

Features

A quick overview of the most important features:

    Easy to use
    Supports any and all gamepad and joystick controllers on your computer
    Even use your keyboard for input
    Turbo Fire!
    Antialiasing and some "next gen" effects for the video stream.
    Great for recording or streaming gameplay footage.
    Open source with light weight license.

    Multi-threaded (receiving the packets and decoding the video stream are on two separate threads) (build 1)
    Supports InputRedirection via Luma on the 3DS. (build 1)
    Integrated touch-screen redirect into the bottom screen output. (build 1)
    3D cursor for touch-screen when in a perspective projected 3D display mode. (build 1)
    Supports any and all game pads your operating system supports out of the box (build 1)
    All redirected controls configurable to keyboard, mouse, and gamepad buttons and axes. (build 1)
    Full screen and windowed mode (build 1)
    Easily resizable windowed mode (no having to restart with the command line like regular NTR Viewer) (build 1)
    When virtual 3DS mode, the Dpad, A, B, X, Y, Start, Select, and Home buttons can all be clicked on with the mouse without having to configure any controls (build 1)
    Automatically detects IP of 3DS for input redirection if RemotePlay has already been started on the 3DS (build 1)
    Supports New3DSXL's c-stick and ZR/ZL buttons (build 1)
    Two display modes. Virtual 3DS and Draggable Resizable Screens. Switch between them by pressing the display change button (build 3)
    Screen size and position saved between sessions (build 7)
    In virtual 3DS display mode, the buttons glow when pressed, to show input when recording (build 8)
    Automatically finds your 3DS as long as bootNTR and Luma input redirection are running (build 8)
    Configurable stream quality settings. (build 9)
    Default stream quality settings produce acceptable quality and framerate for both top and bottom screen. (build 1)
    Turbo fire(build 9)
    Antialiasing (build 9)
    Post processing (build 9)
    Pixel perfect accuracy on touch screen (build 10)
    Cycle through different camera angles on Virtual 3DS mode. (build 10)
    Antialiasing is done on the stream directly (build 10)


How To Use

Step 1: Run bootNTR Selector on your 3DS

Step 2: Enable input redirection on Luma.

Step 3: Run Twix

Step 4: If Twix doesn't know the correct IP address for your 3DS, hit Start Stream to find it. Otherwise hit Try IP.

Step 5: Have fun!


When in Virtual 3DS Mode, use the mouse wheel to cycle between camera angles.


In either mode, hit the Escape key to pull up the options window.

I hit Start Stream and Twix never finds my 3DS

If Start Stream never finds your 3DS then either your 3DS isn't on the same router as your computer, the 3DS doesn't have its wifi on, bootNTR and Luma aren't running, or your router is assigning your 3DS an IP that isn't in your computer's range. For example if your computer's LAN address is 192.168.1.2 and your 3DS is on 192.168.1.4 then Start Stream will very quickly find your 3DS. However if your 3DS is on 192.168.2.x, instead of 192.168.1.whatever, then it will not find it and you'll have to manually enter the IP yourself and use "Try IP" button.


By the way, it doesn't matter what your IP is. IPs are arranged like A.B.C.D, and as long as the A, B, and C are the same for the 3DS and the computer Twix is on, it should find it. The higher the D is on the 3DS the longer it'll take, though.


The stream output is glitchy and the frames keep flashing

This happens because of network lag and there's nothing Twix nor any other stream receiver can really do about it. Check to make sure your 3DS and router are close enough, close off any torents or high bandwidth apps on your computer, make sure nothing else is using your router for high bandwidth stuff. Basically the usual slow wifi debugging stuff.


If none of that works, try reducing the jpeg quality or otherwise adjusting the quality settings whens starting a new stream. Ideally, there would be some manner of telling the bootNTR to lower its quality in these situations. I've tried doing that, but it seems that once the stream is started, it can't be adjusted without restarting the 3DS.


The only reason other streamers don't have this problem is they're all using the reference code for interpretting the stream, which drops any incomplete or out of order frames entirely. I rolled my own code by reverse engineering the packets, and I'd personally rather have a glitchy display than one that appears frozen.[/spoiler]

Known Bugs

Resizable, Draggable screens display mode has touch screen axes being wrong. Please use virtual 3DS mode only for now if you need to use the touch screen. I have a fix already and will be pushing it out with Build 11.

Screenshots and Videos

http://www.youtube.com/watch?v=0kVZrOUc1Qw


http://www.youtube.com/watch?v=AgAgDMZiXMo


http://www.youtube.com/watch?v=0gLN7d8pDV4

FAQ
There aren't many questions I've gotten about it so far but there is one that I keep being asked on other webpages and in private.


Q: Why did you make Twix when there's similar programs available?


A: This question was mainly asked at the very start, when I hadn't added all the extra features I've developed. The main reason is that I wanted to up the ante. I wanted better and knew I could do better. I also hope that Twix inspires the others to do better.