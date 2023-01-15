# AppSpaces
Desktop spaces for your apps

## WiP
This is once again just a PoC based on an idea that popped in my head. Whether I will finish this or not is to be seen...

## Definitions
- An App is an identifier to find an app, this can be by title or executable path.
- A Space is an area on the screen and all the Apps assigned to it.
    An App can only be placed on a space once.
- An AppSpace is a collection of Spaces
    - An AppSpace cannot contain any duplicate apps, 1 app can be placed in 1 of its spaces only.
- You can define multiple AppSpaces

## Use cases
- When a new window is opened it should be placed in its designated space
- When a window is dropped in a space it is maximized within that space.
    - A question is asked whether the app should be linked to that space.
        - If if was already linked to another space it is moved.
- When an app is maximized it is maximized within the space the click was in.
- When a different AppSpace is loaded all windows are moved to their assigned space.
- By default the first space is designated as the primary space
    - Any apps that are opened which have not been assigned to a space will open in the primary space.
    - The user can select 1 other space per AppSpace as the primary space.
    - If the primary space changes all apps that have not been assiged to a space will be moved to the primary space.

## Third party licenses
This project uses WinMan, H.NotifyIcon and H.Hooks. See the linked [Third Party License](/Third%20Party%20License.md) file for more details