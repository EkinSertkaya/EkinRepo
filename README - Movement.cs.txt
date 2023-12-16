- Install "Input System" package from package manager.

- Decrease gravity setting to avoid floaty movement. ( -25 works well with below numbers.)

- Make sure your character game object has BoxCollider2D and Rigidbody2D as components.

- On Rigidbody2D component, set freeze rotation on Z to true.

- Make sure that you set the Interpolate field of the Rigidbody2D component to Interpolate

- Add the Movement.cs to the character.

- Create an empty game object called "GroundCheck" as a child of the character and position it to the bottom-center of the collider.

- Drag and drop this GroundCheck object to the Ground Check field of the Movement component.

- Adding the Ground Check creates a rectangle shaped gizmo on the editor screen. Position this rectangle in a way that only a very small portion of it sticks out of the character collider.
Keep it narrow enough so that it wont conflict with the wall checker. You can manipulate the size by using the Ground Check Size filed on the Movement component.

- Create an empty game object called "RightWallCheck"as a child of the character and position it to the right of the collider.

- Drag and drop this RightWallCheck object to the Right Wall Check field of the Movement component.

- Adding the Right Wall Check creates a sphere shaped gizmo on the editor screen. Position this sphere in a way that only a very small portion of it sticks out of the character collider.

- Drag and drop PlayerControls file to Player Actions field of the Movement component

- Add a layer called "Ground".

- Set the Ground Check Layers field on the Movement component to "Ground".

- Set your ground game objects' layers to "Ground"

- Add you character game object to Rb field of the Movement component.

- Add a 0 friction Physics Material 2D to the Material field of the BoxCollider2D component.

You can use below numbers as a starting(tweaking) point in -25 gravity.	

RUN
Max Speed: 5
Acceleration: 50
Decceleration: 50

JUMP
Jump Force: 12
Air Acceleration: 15
Air Decceleration: 15
Minimum Jump Height: 0.1
Gravity Scale: 1.7
Max Fall Speed: 15
Jump Input Buffer: 0.1
Coyote Time: 0.1

AIR JUMP
Air Jump Height: 12
Air Jump Limit: 1

WALL JUMP
Wall Jump Horizontal Force: 20
Wall Jump Vertical Force: 12
Wall Jump Min Jump Height: 0.1
Wall Sliding Max Fall Speed: 5
Wall Sliding Gravity Scale: 0.5

DASH
Dash Limit: 1
Dash Speed: 6.5
Dash Duration: 0.2







