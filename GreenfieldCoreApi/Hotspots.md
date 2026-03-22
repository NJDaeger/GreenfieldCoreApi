# Hotspot System

Define points of interest on a Minecraft map.

# Database Schema

### Hotspot
The main entity representing a point of interest on the map. It has no spatial data itself, but serves as a parent for 
HotspotInstances which define the actual location and orientation on the map.

| ColumnName | DataType | Constraints           |
|------------|----------|-----------------------|
| hotspotId  | bigint   | PK                    |
| createdBy  | bigint   | FK Users.Users.userId |
| createdOn  | datetime |                       |

### MapVersion
The entity representing a specific version of a Minecraft map. Each version can have multiple HotspotInstances associated with it.

| ColumnName      | DataType     | Constraints |
|-----------------|--------------|-------------|
| mapVersionId    | bigint       | PK          |
| mapVersionName  | nvarchar(64) | UQ          |
| mapVersionOrder | int          |             |
| worldUuid       | char(36)     |             |

### DetailType
Defines the types of details that can be associated with a HotspotInstance, such as "Description", "Image URL", etc.

| ColumnName            | DataType     | Constraints |
|-----------------------|--------------|-------------|
| detailTypeId          | bigint       | PK          |
| detailTypeCode        | nvarchar(64) | UQ          |
| detailTypeDisplayName | nvarchar(64) | utf8mb4     |

### HotspotInstance
Represents a specific instance of a Hotspot on a particular map version. It contains the spatial data (x, y, z coordinates and orientation).
The instance itself has no direct relationship to a MapVersion, but is linked through the HotspotInstanceMapVersion table to allow for many-to-many relationships (a single instance can be associated with multiple map versions if needed).
When a hotspot needs to change location, parent, or any detail property, a new HotspotInstance is created and linked to the same Hotspot. This allows a user to see the history of a given hotspot across versions.

| ColumnName        | DataType      | Constraints                  |
|-------------------|---------------|------------------------------|
| hotspotInstanceId | bigint        | PK                           |
| hotspotId         | bigint        | FK Hotspot.Hotspot.hotspotId |
| parentHotspotId   | bigint null   | FK Hotspot.Hotspot.hotspotId |
| x                 | int           |                              |
| y                 | int           |                              |
| z                 | int           |                              |
| yaw               | decimal(5,2)  |                              |
| pitch             | decimal(5,2)  |                              |
| createdBy         | bigint        | FK Users.Users.userId        |
| createdOn         | datetime      |                              |

### HotspotInstanceMapVersion
Links a hotspot instance to a specific map version. Basically says "this hotspot instance is relevant for this map version". This allows for a single hotspot instance to be associated with multiple map versions if needed, and also allows for tracking the history of a hotspot across different versions of the map.

| ColumnName        | DataType | Constraints                                              |
|-------------------|----------|----------------------------------------------------------|
| hotspotInstanceId | bigint   | PK, FK HotspotInstance.HotspotInstance.hotspotInstanceId |
| mapVersionId      | bigint   | PK, FK MapVersion.MapVersion.mapVersionId                |

### HotspotInstanceDetail
Represents a specific detail associated with a HotspotInstance, such as a description, image URL, etc. The type of detail is defined by the DetailType entity.

| ColumnName              | DataType       | Constraints                                          |
|-------------------------|----------------|------------------------------------------------------|
| hotspotInstanceDetailId | bigint         | PK                                                   |
| hotspotInstanceId       | bigint         | FK HotspotInstance.HotspotInstance.hotspotInstanceId |
| detailTypeId            | bigint         | FK DetailType.DetailType.detailTypeId                |
| detailValue             | nvarchar(1024) | utf8mb4                                              |
| createdBy               | bigint         | FK Users.Users.userId                                |
| createdOn               | datetime       |                                                      |

### HotspotBuilder
Represents the builders associated with a Hotspot. A builder is a user who has contributed to the creation or modification of a hotspot. This table allows for tracking which users have been involved in the development of a hotspot.
Notably not linked to a specific instance, because if a builder starts a hotspot and then it gets moved or changed, they should still be credited as a builder of that hotspot.

| ColumnName | DataType | Constraints                      |
|------------|----------|----------------------------------|
| hotspotId  | bigint   | PK, FK Hotspot.Hotspot.hotspotId |
| userId     | bigint   | PK, FK Users.Users.userId        |

# General Concepts
* When a hotspot needs to be modified, a new HotspotInstance