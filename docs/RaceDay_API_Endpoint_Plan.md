RaceDay API Endpoint Plan
Authentication Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
POST	/api/auth/register	Register a new user account	None (Public)	{ email, password, firstName, lastName, role }	201 Created - user details; 400 Bad Request - validation errors; 409 Conflict - email already exists
POST	/api/auth/login	Authenticate a user and return JWT token	None (Public)	{ email, password }	200 OK - { token, user }; 401 Unauthorized - invalid credentials
User Profile Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
GET	/api/users/profile	Get the authenticated user's profile	Any (Logged in)	None	200 OK - user profile; 401 Unauthorized; 404 Not Found
PUT	/api/users/profile	Update the authenticated user's profile	Any (Logged in)	{ firstName, lastName, email }	200 OK - updated profile; 400 Bad Request; 401 Unauthorized
Event Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
GET	/api/events	Get all events (with optional filters)	Any (Logged in)	None	200 OK - list of events; 401 Unauthorized
GET	/api/events/{id}	Get a specific event by ID	Any (Logged in)	None	200 OK - event details; 404 Not Found
POST	/api/events	Create a new event	Organiser only	{ eventName, description, eventDate, location, distance, eventType, maxParticipants }	201 Created - event details; 400 Bad Request; 403 Forbidden
PUT	/api/events/{id}	Update an existing event	Organiser only	{ eventName, description, eventDate, location, distance, eventType, maxParticipants }	200 OK - updated event; 400 Bad Request; 403 Forbidden; 404 Not Found
DELETE	/api/events/{id}	Delete an event	Organiser only	None	204 No Content; 403 Forbidden; 404 Not Found
Category Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
GET	/api/events/{id}/categories	Get all categories for an event	Any (Logged in)	None	200 OK - list of categories; 404 Not Found
POST	/api/categories	Create a new category for an event	Organiser only	{ eventId, categoryName, description, entryFee, startTime }	201 Created - category details; 400 Bad Request; 403 Forbidden; 404 Not Found
PUT	/api/categories/{id}	Update an existing category	Organiser only	{ categoryName, description, entryFee, startTime }	200 OK - updated category; 400 Bad Request; 403 Forbidden; 404 Not Found
DELETE	/api/categories/{id}	Delete a category	Organiser only	None	204 No Content; 403 Forbidden; 404 Not Found
Enrolment Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
GET	/api/events/{id}/enrolments	Get all enrolments for an event	Organiser only	None	200 OK - list of enrolments; 403 Forbidden; 404 Not Found
POST	/api/enrolments	Enrol a participant in an event	Participant only	{ eventId, categoryId }	201 Created - enrolment details; 400 Bad Request; 403 Forbidden; 409 Conflict - already enrolled
GET	/api/enrolments/{id}	Get a specific enrolment	Participant only	None	200 OK - enrolment details; 403 Forbidden; 404 Not Found
GET	/api/users/me/enrolments	Get all enrolments for the authenticated user	Participant only	None	200 OK - list of enrolments; 401 Unauthorized
DELETE	/api/enrolments/{id}	Withdraw from an event	Participant only	None	204 No Content; 403 Forbidden; 404 Not Found
Result Endpoints
HTTP Method	Route	Description	Role Required	Request Body	Expected Response
GET	/api/enrolments/{id}/result	Get the result for a specific enrolment	Participant only	None	200 OK - result details; 403 Forbidden; 404 Not Found
POST	/api/events/{id}/results	Capture results for an event	Organiser only	{ enrolmentId, finishTime, finishPosition }	201 Created - result details; 400 Bad Request; 403 Forbidden; 404 Not Found
PUT	/api/results/{id}	Update an existing result	Organiser only	{ finishTime, finishPosition }	200 OK - updated result; 400 Bad Request; 403 Forbidden; 404 Not Found
GET	/api/users/me/results	Get all results for the authenticated user	Participant only	None	200 OK - list of results; 401 Unauthorized
Endpoint Summary
Category	Endpoints	Description
Authentication	2	Register and Login
User Profile	2	View and Update profile
Events	5	CRUD operations for events
Categories	4	CRUD operations for categories
Enrolments	5	Create, view, and withdraw enrolments
Results	4	Capture, update, and view results
Total	22	
Role Access Summary
Role	Can Access
Public	Register, Login
Any User	View profile, View events, View categories
Participant	Enrol in events, View own enrolments, View own results
Organiser	Create/update/delete events, Manage categories, View enrolments, Capture results
HTTP Status Codes Used
Status Code	Description
200 OK	Request successful
201 Created	Resource created successfully
204 No Content	Request successful, no content to return
400 Bad Request	Invalid request data
401 Unauthorized	Authentication required
403 Forbidden	User does not have permission
404 Not Found	Resource not found
409 Conflict	Resource already exists (e.g., duplicate enrolment)