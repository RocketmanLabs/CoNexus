# Survey API - Complete Endpoint Reference

## Base URL
```
Development: https://localhost:5001/api
Production: https://your-domain.com/api
```

---

## 1. Users Management

### 1.1 Sync Users from CRM

**Purpose:** Bulk create or update users from external CRM system.

**Endpoint:**
```
POST /api/users/sync
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
[
  {
    "externalCrmId": "CRM123",
    "email": "john.doe@company.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true
  },
  {
    "externalCrmId": "CRM456",
    "email": "jane.smith@company.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true
  }
]
```

**Response: 200 OK**
```json
{
  "created": 2,
  "updated": 0,
  "deactivated": 0
}
```

---

### 1.2 Get User by ID

**Purpose:** Retrieve a single user by their ID.

**Endpoint:**
```
GET /api/users/{id}
```

**Route Parameters:**
- `id` (integer, required): User ID

**Example:**
```
GET /api/users/1
```

**Response: 200 OK**
```json
{
  "id": 1,
  "externalCrmId": "CRM123",
  "email": "john.doe@company.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true
}
```

**Response: 404 Not Found**
```json
{
  "error": "User not found"
}
```

---

## 2. Survey Management

### 2.1 Create Survey

**Purpose:** Create a new survey with questions.

**Endpoint:**
```
POST /api/surveys
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Employee Satisfaction Survey",
  "description": "Q1 2025 quarterly survey",
  "questions": [
    {
      "questionText": "How satisfied are you with your role?",
      "type": 1,
      "orderIndex": 0,
      "isRequired": true,
      "choices": ["Very Satisfied", "Satisfied", "Neutral", "Dissatisfied", "Very Dissatisfied"],
      "scaleId": null
    },
    {
      "questionText": "How would you rate your work-life balance?",
      "type": 1,
      "orderIndex": 1,
      "isRequired": true,
      "choices": null,
      "scaleId": 1
    },
    {
      "questionText": "What can we improve?",
      "type": 2,
      "orderIndex": 2,
      "isRequired": false,
      "choices": null,
      "scaleId": null
    }
  ]
}
```

**Question Type Values:**
- `1` = MultipleChoice
- `2` = FreeText

**Notes:**
- For multiple choice questions, provide either `choices` array OR `scaleId`, not both
- `choices`: Array of strings for inline choices
- `scaleId`: Integer ID of a reusable scale

**Response: 201 Created**
```json
{
  "id": 1,
  "title": "Employee Satisfaction Survey",
  "description": "Q1 2025 quarterly survey",
  "isActive": true,
  "questions": [
    {
      "id": 1,
      "questionText": "How satisfied are you with your role?",
      "type": 1,
      "orderIndex": 0,
      "isRequired": true,
      "choices": ["Very Satisfied", "Satisfied", "Neutral", "Dissatisfied", "Very Dissatisfied"]
    },
    {
      "id": 2,
      "questionText": "How would you rate your work-life balance?",
      "type": 1,
      "orderIndex": 1,
      "isRequired": true,
      "choices": []
    },
    {
      "id": 3,
      "questionText": "What can we improve?",
      "type": 2,
      "orderIndex": 2,
      "isRequired": false,
      "choices": []
    }
  ]
}
```

**Location Header:**
```
Location: /api/surveys/1
```

---

### 2.2 Get Survey by ID

**Purpose:** Retrieve a survey with all its questions.

**Endpoint:**
```
GET /api/surveys/{id}
```

**Route Parameters:**
- `id` (integer, required): Survey ID

**Example:**
```
GET /api/surveys/1
```

**Response: 200 OK**
```json
{
  "id": 1,
  "title": "Employee Satisfaction Survey",
  "description": "Q1 2025 quarterly survey",
  "isActive": true,
  "questions": [
    {
      "id": 1,
      "questionText": "How satisfied are you with your role?",
      "type": 1,
      "orderIndex": 0,
      "isRequired": true,
      "choices": ["Very Satisfied", "Satisfied", "Neutral", "Dissatisfied", "Very Dissatisfied"]
    }
  ]
}
```

**Response: 404 Not Found**

---

### 2.3 Get All Surveys

**Purpose:** List all surveys, optionally filtered by active status.

**Endpoint:**
```
GET /api/surveys
```

**Query Parameters:**
- `activeOnly` (boolean, optional, default: true): Filter by active status

**Examples:**
```
GET /api/surveys
GET /api/surveys?activeOnly=false
```

**Response: 200 OK**
```json
[
  {
    "id": 1,
    "title": "Employee Satisfaction Survey",
    "description": "Q1 2025 quarterly survey",
    "isActive": true,
    "questions": [...]
  },
  {
    "id": 2,
    "title": "Customer Feedback Survey",
    "description": "Monthly feedback",
    "isActive": true,
    "questions": [...]
  }
]
```

---

### 2.4 Update Survey

**Purpose:** Update an existing survey and its questions.

**Endpoint:**
```
PUT /api/surveys/{id}
```

**Route Parameters:**
- `id` (integer, required): Survey ID

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Updated Survey Title",
  "description": "Updated description",
  "isActive": true,
  "questions": [
    {
      "questionText": "Updated question text?",
      "type": 1,
      "orderIndex": 0,
      "isRequired": true,
      "choices": ["Option 1", "Option 2", "Option 3"],
      "scaleId": null
    }
  ]
}
```

**Notes:**
- Updating a survey replaces all questions
- Set `isActive` to `false` to deactivate without deletion

**Response: 200 OK**
```json
{
  "id": 1,
  "title": "Updated Survey Title",
  "description": "Updated description",
  "isActive": true,
  "questions": [...]
}
```

**Response: 404 Not Found**

---

### 2.5 Delete Survey

**Purpose:** Soft delete (deactivate) a survey.

**Endpoint:**
```
DELETE /api/surveys/{id}
```

**Route Parameters:**
- `id` (integer, required): Survey ID

**Example:**
```
DELETE /api/surveys/1
```

**Response: 204 No Content**

**Response: 404 Not Found**

---

## 3. Publication Management

### 3.1 Publish Survey

**Purpose:** Make a survey available for responses (called by CRM).

**Endpoint:**
```
POST /api/publications
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "surveyId": 1,
  "publicationName": "Q1 2025 Employee Survey - January"
}
```

**Response: 201 Created**
```json
{
  "id": 1,
  "surveyId": 1,
  "publicationName": "Q1 2025 Employee Survey - January",
  "publishedAt": "2025-01-15T10:00:00Z",
  "closedAt": null,
  "isOpen": true
}
```

**Location Header:**
```
Location: /api/publications/1
```

**Response: 404 Not Found**
```json
{
  "error": "Survey not found"
}
```

**Response: 400 Bad Request**
```json
{
  "error": "Cannot publish inactive survey"
}
```

---

### 3.2 Get Publication by ID

**Purpose:** Retrieve publication details.

**Endpoint:**
```
GET /api/publications/{id}
```

**Route Parameters:**
- `id` (integer, required): Publication ID

**Example:**
```
GET /api/publications/1
```

**Response: 200 OK**
```json
{
  "id": 1,
  "surveyId": 1,
  "publicationName": "Q1 2025 Employee Survey - January",
  "publishedAt": "2025-01-15T10:00:00Z",
  "closedAt": null,
  "isOpen": true
}
```

**Response: 404 Not Found**

---

### 3.3 Close Publication

**Purpose:** Close a publication to prevent further responses.

**Endpoint:**
```
POST /api/publications/{id}/close
```

**Route Parameters:**
- `id` (integer, required): Publication ID

**Example:**
```
POST /api/publications/1/close
```

**Response: 200 OK**
```json
{
  "message": "Publication closed successfully"
}
```

**Response: 404 Not Found**

**Response: 400 Bad Request**
```json
{
  "error": "Publication is already closed"
}
```

---

## 4. Response Submission

### 4.1 Submit Responses

**Purpose:** Submit user responses to survey questions.

**Endpoint:**
```
POST /api/responses
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "publicationId": 1,
  "userId": 1,
  "answers": [
    {
      "questionId": 1,
      "answer": "Very Satisfied"
    },
    {
      "questionId": 2,
      "answer": "Agree"
    },
    {
      "questionId": 3,
      "answer": "Better communication and more flexible hours would be appreciated."
    }
  ]
}
```

**Notes:**
- All required questions must be answered
- Answers must match available choices for multiple choice questions
- For questions using scales, answer must match a choice text from the scale
- Submitting again for same user/question/publication will update the response

**Response: 200 OK**
```json
{
  "message": "Responses submitted successfully"
}
```

**Response: 400 Bad Request**
```json
{
  "errors": [
    "User not found",
    "Publication is closed",
    "Required question not answered: How satisfied are you with your role?",
    "Invalid answer for question: How would you rate your work-life balance?"
  ]
}
```

---

### 4.2 Get User Progress

**Purpose:** Check how many questions a user has answered.

**Endpoint:**
```
GET /api/responses/progress
```

**Query Parameters:**
- `userId` (integer, required): User ID
- `publicationId` (integer, required): Publication ID

**Example:**
```
GET /api/responses/progress?userId=1&publicationId=1
```

**Response: 200 OK**
```json
{
  "totalQuestions": 10,
  "answeredQuestions": 7,
  "percentComplete": 70.0,
  "unansweredQuestionIds": [4, 8, 10]
}
```

**Response: 404 Not Found**

---

## 5. Reporting & Analytics

### 5.1 Get User Report

**Purpose:** Get all responses from a specific user for a publication.

**Endpoint:**
```
GET /api/reports/user/{userId}/publication/{publicationId}
```

**Route Parameters:**
- `userId` (integer, required): User ID
- `publicationId` (integer, required): Publication ID

**Example:**
```
GET /api/reports/user/1/publication/1
```

**Response: 200 OK**
```json
{
  "userId": 1,
  "userName": "John Doe",
  "publicationId": 1,
  "responses": [
    {
      "questionText": "How satisfied are you with your role?",
      "questionType": 1,
      "answer": "Very Satisfied",
      "respondedAt": "2025-01-15T14:30:00Z"
    },
    {
      "questionText": "What can we improve?",
      "questionType": 2,
      "answer": "Better communication channels",
      "respondedAt": "2025-01-15T14:31:00Z"
    }
  ]
}
```

**Response: 404 Not Found**

---

### 5.2 Get Question Statistics

**Purpose:** Get aggregated statistics for a specific question.

**Endpoint:**
```
GET /api/reports/question/{questionId}/publication/{publicationId}
```

**Route Parameters:**
- `questionId` (integer, required): Question ID
- `publicationId` (integer, required): Publication ID

**Example:**
```
GET /api/reports/question/1/publication/1
```

**Response: 200 OK (Multiple Choice)**
```json
{
  "questionId": 1,
  "questionText": "How satisfied are you with your role?",
  "questionType": 1,
  "n": 150,
  "mode": ["Very Satisfied"],
  "choiceDistribution": [
    {
      "choice": "Very Satisfied",
      "count": 75,
      "percentage": 50.0
    },
    {
      "choice": "Satisfied",
      "count": 50,
      "percentage": 33.33
    },
    {
      "choice": "Neutral",
      "count": 20,
      "percentage": 13.33
    },
    {
      "choice": "Dissatisfied",
      "count": 5,
      "percentage": 3.33
    },
    {
      "choice": "Very Dissatisfied",
      "count": 0,
      "percentage": 0.0
    }
  ],
  "textResponses": null
}
```

**Response: 200 OK (Free Text)**
```json
{
  "questionId": 3,
  "questionText": "What can we improve?",
  "questionType": 2,
  "n": 150,
  "mode": null,
  "choiceDistribution": null,
  "textResponses": [
    "Better communication channels",
    "More flexible working hours",
    "Improved workspace ergonomics",
    "..."
  ]
}
```

**Response: 404 Not Found**

---

### 5.3 Get Survey Statistics

**Purpose:** Get aggregated statistics for all questions in a survey.

**Endpoint:**
```
GET /api/reports/survey/{surveyId}/publication/{publicationId}
```

**Route Parameters:**
- `surveyId` (integer, required): Survey ID
- `publicationId` (integer, required): Publication ID

**Example:**
```
GET /api/reports/survey/1/publication/1
```

**Response: 200 OK**
```json
{
  "surveyId": 1,
  "surveyTitle": "Employee Satisfaction Survey",
  "publicationId": 1,
  "publicationName": "Q1 2025 Employee Survey - January",
  "totalUsers": 200,
  "respondedUsers": 150,
  "responseRate": 75.0,
  "questionStatistics": [
    {
      "questionId": 1,
      "questionText": "How satisfied are you with your role?",
      "questionType": 1,
      "n": 150,
      "mode": ["Very Satisfied"],
      "choiceDistribution": [...]
    },
    {
      "questionId": 2,
      "questionText": "How would you rate your work-life balance?",
      "questionType": 1,
      "n": 150,
      "mode": ["Good"],
      "choiceDistribution": [...]
    }
  ]
}
```

**Response: 404 Not Found**

---

### 5.4 Export to CSV

**Purpose:** Export all publication responses as CSV file.

**Endpoint:**
```
GET /api/reports/export/publication/{publicationId}
```

**Route Parameters:**
- `publicationId` (integer, required): Publication ID

**Example:**
```
GET /api/reports/export/publication/1
```

**Response: 200 OK**
```
Content-Type: text/csv
Content-Disposition: attachment; filename="survey_results_1.csv"

User Email,User Name,Question,Question Type,Answer,Responded At
john.doe@company.com,John Doe,"How satisfied are you with your role?",MultipleChoice,"Very Satisfied","2025-01-15 14:30:00"
john.doe@company.com,John Doe,"What can we improve?",FreeText,"Better communication","2025-01-15 14:31:00"
jane.smith@company.com,Jane Smith,"How satisfied are you with your role?",MultipleChoice,"Satisfied","2025-01-15 15:00:00"
```

**Response: 404 Not Found**

---

## 6. Scale Management

### 6.1 Create Scale

**Purpose:** Create a reusable scale with choices.

**Endpoint:**
```
POST /api/scales
```

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "5-Point Likert Scale",
  "choices": [
    {
      "text": "Strongly Disagree",
      "sequence": 1,
      "value": 1
    },
    {
      "text": "Disagree",
      "sequence": 2,
      "value": 2
    },
    {
      "text": "Neutral",
      "sequence": 3,
      "value": 3
    },
    {
      "text": "Agree",
      "sequence": 4,
      "value": 4
    },
    {
      "text": "Strongly Agree",
      "sequence": 5,
      "value": 5
    }
  ]
}
```

**Field Descriptions:**
- `text`: Display text (max 50 characters)
- `sequence`: Display order (integer)
- `value`: Numeric value for scoring/analysis

**Response: 201 Created**
```json
{
  "id": 1,
  "title": "5-Point Likert Scale",
  "choices": [
    {
      "id": 1,
      "text": "Strongly Disagree",
      "sequence": 1,
      "value": 1
    },
    {
      "id": 2,
      "text": "Disagree",
      "sequence": 2,
      "value": 2
    },
    {
      "id": 3,
      "text": "Neutral",
      "sequence": 3,
      "value": 3
    },
    {
      "id": 4,
      "text": "Agree",
      "sequence": 4,
      "value": 4
    },
    {
      "id": 5,
      "text": "Strongly Agree",
      "sequence": 5,
      "value": 5
    }
  ],
  "questionCount": 0
}
```

**Location Header:**
```
Location: /api/scales/1
```

---

### 6.2 Get Scale by ID

**Purpose:** Retrieve a scale with all its choices.

**Endpoint:**
```
GET /api/scales/{id}
```

**Route Parameters:**
- `id` (integer, required): Scale ID

**Example:**
```
GET /api/scales/1
```

**Response: 200 OK**
```json
{
  "id": 1,
  "title": "5-Point Likert Scale",
  "choices": [
    {
      "id": 1,
      "text": "Strongly Disagree",
      "sequence": 1,
      "value": 1
    },
    {
      "id": 2,
      "text": "Disagree",
      "sequence": 2,
      "value": 2
    },
    {
      "id": 3,
      "text": "Neutral",
      "sequence": 3,
      "value": 3
    },
    {
      "id": 4,
      "text": "Agree",
      "sequence": 4,
      "value": 4
    },
    {
      "id": 5,
      "text": "Strongly Agree",
      "sequence": 5,
      "value": 5
    }
  ],
  "questionCount": 5
}
```

**Response: 404 Not Found**

---

### 6.3 Get All Scales

**Purpose:** List all scales, optionally with usage counts.

**Endpoint:**
```
GET /api/scales
```

**Query Parameters:**
- `includeUsageCount` (boolean, optional, default: false): Include question usage count

**Examples:**
```
GET /api/scales
GET /api/scales?includeUsageCount=true
```

**Response: 200 OK**
```json
[
  {
    "id": 1,
    "title": "5-Point Likert Scale",
    "choices": [
      {
        "id": 1,
        "text": "Strongly Disagree",
        "sequence": 1,
        "value": 1
      },
      {
        "id": 2,
        "text": "Disagree",
        "sequence": 2,
        "value": 2
      },
      {
        "id": 3,
        "text": "Neutral",
        "sequence": 3,
        "value": 3
      },
      {
        "id": 4,
        "text": "Agree",
        "sequence": 4,
        "value": 4
      },
      {
        "id": 5,
        "text": "Strongly Agree",
        "sequence": 5,
        "value": 5
      }
    ],
    "questionCount": 5
  },
  {
    "id": 2,
    "title": "7-Point Scale",
    "choices": [...],
    "questionCount": 2
  }
]
```

---

### 6.4 Update Scale

**Purpose:** Update a scale and all its choices.

**Endpoint:**
```
PUT /api/scales/{id}
```

**Route Parameters:**
- `id` (integer, required): Scale ID

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Updated 5-Point Likert Scale",
  "choices": [
    {
      "text": "Strongly Disagree",
      "sequence": 1,
      "value": 1
    },
    {
      "text": "Disagree",
      "sequence": 2,
      "value": 2
    },
    {
      "text": "Neutral",
      "sequence": 3,
      "value": 3
    },
    {
      "text": "Agree",
      "sequence": 4,
      "value": 4
    },
    {
      "text": "Strongly Agree",
      "sequence": 5,
      "value": 5
    }
  ]
}
```

**Notes:**
- Updating a scale replaces all choices
- Changes affect all questions using this scale

**Response: 200 OK**
```json
{
  "id": 1,
  "title": "Updated 5-Point Likert Scale",
  "choices": [...],
  "questionCount": 5
}
```

**Response: 404 Not Found**

---

### 6.5 Delete Scale

**Purpose:** Delete a scale (only if not in use).

**Endpoint:**
```
DELETE /api/scales/{id}
```

**Route Parameters:**
- `id` (integer, required): Scale ID

**Example:**
```
DELETE /api/scales/1
```

**Response: 204 No Content**

**Response: 404 Not Found**

**Response: 400 Bad Request**
```json
{
  "error": "Scale with ID 1 is in use and cannot be deleted"
}
```

---

## HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, POST (non-creation) |
| 201 | Created | Successful POST with resource creation |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation error, business rule violation |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Unexpected server error |

---

## Common Error Response Format

```json
{
  "error": "Error message describing what went wrong"
}
```

or

```json
{
  "errors": [
    "Error message 1",
    "Error message 2"
  ]
}
```

---

## Authentication

**Current:** No authentication (development mode)

**Production:** Will require authentication headers
```
Authorization: Bearer {jwt_token}
```

---

## Rate Limiting

**Current:** No rate limiting (development mode)

**Production:** Will be implemented based on requirements

---

## Pagination

**Current:** Not implemented

**Future:** Large result sets will support pagination
```
GET /api/surveys?page=1&pageSize=20
```

---

## Testing with cURL

### Create Survey
```bash
curl -X POST https://localhost:5001/api/surveys \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Survey",
    "description": "Test Description",
    "questions": [
      {
        "questionText": "Test Question?",
        "type": 1,
        "orderIndex": 0,
        "isRequired": true,
        "choices": ["Yes", "No"],
        "scaleId": null
      }
    ]
  }'
```

### Submit Response
```bash
curl -X POST https://localhost:5001/api/responses \
  -H "Content-Type: application/json" \
  -d '{
    "publicationId": 1,
    "userId": 1,
    "answers": [
      {
        "questionId": 1,
        "answer": "Yes"
      }
    ]
  }'
```

### Export CSV
```bash
curl -X GET https://localhost:5001/api/reports/export/publication/1 \
  -o survey_results.csv
```

---

## Swagger/OpenAPI Documentation

Interactive API documentation available at:
```
https://localhost:5001/swagger
```

Features:
- Try out endpoints directly
- View request/response schemas
- Download OpenAPI specification

---

## Summary

**Total Endpoints:** 18

| Resource | Endpoints |
|----------|-----------|
| Users | 2 |
| Surveys | 5 |
| Publications | 3 |
| Responses | 2 |
| Reports | 4 |
| Scales | 5 |

**Supported Operations:**
- ✅ User synchronization from CRM
- ✅ Survey CRUD operations
- ✅ Survey publication and closure
- ✅ Response submission and tracking
- ✅ Statistical reporting and analytics
- ✅ CSV export
- ✅ Reusable scale management