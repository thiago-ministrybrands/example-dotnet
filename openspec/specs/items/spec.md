# Items

## Purpose

The items API stores named items in Postgres and serves a cached item count
from Redis.

## Requirements

### Requirement: Item names are non-blank
An item's name SHALL contain at least one non-whitespace character. `POST /items`
SHALL reject a missing, empty or whitespace-only name with `400 Bad Request`, and a
newly constructed `Item` SHALL NOT default to a whitespace-only name.

#### Scenario: Whitespace-only name is rejected
- **WHEN** a client sends `POST /items` with `{ "name": "   " }`
- **THEN** the API responds `400 Bad Request`
- **AND** no item is stored

#### Scenario: Valid name is stored
- **WHEN** a client sends `POST /items` with `{ "name": "widget" }`
- **THEN** the API responds `201 Created` with the stored item
- **AND** the `Location` header is `/items/{id}`

### Requirement: Items are listed in insertion order
`GET /items` SHALL return every stored item ordered by ascending `Id`.

#### Scenario: Two items are listed in order
- **WHEN** items "first" and then "second" are created
- **THEN** `GET /items` returns "first" before "second"

### Requirement: Item count is cached
`GET /items/count` SHALL return `{ "count": <n> }`, serving the count from Redis
when cached and caching a freshly computed count for 30 seconds otherwise.

#### Scenario: Cache hit skips the database
- **WHEN** the count is cached
- **THEN** the cached value is returned without querying Postgres

### Requirement: Health endpoint
`GET /health` SHALL respond `200 OK` with `{ "status": "ok" }` once the API is running.

#### Scenario: Health check
- **WHEN** a client sends `GET /health`
- **THEN** the API responds `200 OK` with body `{ "status": "ok" }`
