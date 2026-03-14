# Redblock System

Marking and tagging locations in a Minecraft world.

# Database Schema

### Redblock.Project
| Column      | DataType    | Constraints |
|-------------|-------------|-------------|
| projectId   | bigint      | PK          |
| projectName | varchar(64) | utf8mb4 UQ  | 
| projectKey  | varchar(6)  | utf8mb4 UQ  |

### Redblock.Redblock
| Column     | DataType       | Constraints                            |
|------------|----------------|----------------------------------------|
| redblockId | bigint         | PK                                     |
| message    | nvarchar(1024) | utf8mb4                                |
| projectId  | bigint         | FK Redblock.Project                    |
| keyNumber  | bigint         | autoincremet UQ (redblockId,projectId) | 
| x          | int            |                                        |
| y          | int            |                                        |
| z          | int            |                                        |
| createdBy  | bigint         | FK Users.Users                         | 
| createdOn  | datetime       |                                        |
| deletedBy  | bigint         | FK Users.Users                         |
| deletedOn  | datetime       |                                        |

The combination of redblockId and projectId is unique, and keyNumber is an auto-incrementing number that resets for each project. This allows for a simple way to reference redblocks within a project using the keyNumber.

### Redblock.Status
| Column    | DataType     | Constraints    |
|-----------|--------------|----------------|
| statusId  | bigint       | PK             |
| redblockId | bigint       | FK Redblock.Redblock |
| status    | nvarchar(32) | utf8mb4        |
| createdBy | bigint       | FK Users.Users |
| createdOn | datetime     |                |

### Redblock.UserAssignment
| Column     | DataType | Constraints          |
|------------|----------|----------------------|
| redBlockId | bigint   | FK Redblock.Redblock |
| userId     | bigint   | FK Users.Users       |
| createdBy  | bigint   | FK Users.Users       |
| createdOn  | datetime |                      |

### Redblock.RoleAssignment
| Column     | DataType     | Constraints          |
|------------|--------------|----------------------|
| redblockId | bigint       | FK Redblock.Redblock |
| roleName   | nvarchar(32) | utf8mb4              |
| createdBy  | bigint       | FK Users.Users       |
| createdOn  | datetime     |                      |


# API Endpoints
#### Project Endpoints
* `/redblocks/projects` - GET: List all projects
* `/redblocks/projects` - POST: Create a new project
* `/redblocks/projects/{projectId}` - GET: Get project details
* `/redblocks/projects/{projectId}` - PUT: Update project details (no updating the key)

#### Redblock Endpoints
* `/redblocks/projects/{projectId}/redblocks` - GET: List all redblocks in a project
  * Should be paginated.
  * Body should include filters for status, assigned users, assigned roles.
* `/redblocks/projects/{projectId}/redblock` - POST: Create a new redblock in a project
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}` - GET: Get redblock details by key number
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}` - PUT: Update redblock message
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}/status` - POST: Add a status update to a redblock
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}/users` - POST: Assign a user to a redblock
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}/roles` - POST: Assign a role to a redblock
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}` - DELETE: Mark a redblock as deleted (soft delete)
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}/users/{userId}` - DELETE: Remove a user assignment from a redblock
* `/redblocks/projects/{projectId}/redblocks/{keyNumber}/roles/{roleName}` - DELETE: Remove a role assignment from a redblock

