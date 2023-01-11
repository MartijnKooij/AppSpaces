# AppSpaces
Desktop spaces for your apps

## WiP
This is once again just a PoC based on an idea that popped in my head. Whether I will finish this or not is to be seen...

## Definitions
- A space is an area on the screen.
- An AppSpace in an application linked to an app and all its windows.
- An AppSpace cannot contain any duplicate apps, 1 app can be placed in 1 space only.
- You can define multiple AppSpaces

## Use cases
- When a new window is opened it should be placed in its designated space
- When a window is dropped in a space it is maximized within that space.
    - A question is asked whether the app should be linked to that space.
        - If if was already linked to another space it is moved.
- When an app is maximized it is maximized within the space the click was in. Optionally?

## Third party licenses
This project uses WinMan, below is its MIT license.
I compiled the project myself as the NuGet package was not working.
https://github.com/FancyWM/winman-windows

MIT License

Copyright (c) 2021 Veselin Karaganev

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
