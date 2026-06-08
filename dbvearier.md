# DBeaver Survey Response Debugging

Use this guide when final employer registration fails because `CompleteRegistration()` returns backend rule violations. The goal is to compare what the UI saved with what the database contains for the same `SurveyResponseSK`.

## 1. Open A SQL Editor

In DBeaver:

1. Select the database connection, for example `DEV10`.
2. Click the `SQL` button in the top toolbar.
3. Choose `New SQL Script`.
4. Make sure the correct connection/schema is selected in the script tab.
5. Paste one query at a time.
6. Press `Ctrl+Enter` to execute the current query.

## 2. Get SurveyResponseSK From Debug

In Visual Studio, put a breakpoint in:

`EmployerRegistrationModelStore.CompleteRegistration()`

Check this value:

```csharp
EmployerRegistrationModel.SurveyResponseSk
```

Use that value in the SQL queries below.

## 3. Find Related Tables

Run this first to find survey/response tables:

```sql
SELECT table_name
FROM all_tables
WHERE owner = 'USERS'
AND (
    UPPER(table_name) LIKE '%SURVEY%'
    OR UPPER(table_name) LIKE '%RESPONSE%'
    OR UPPER(table_name) LIKE '%QUESTION%'
    OR UPPER(table_name) LIKE '%REGISTRATION%'
)
ORDER BY table_name;
```

## 4. Find Important Columns

Run this to find tables that store survey response keys, question item keys, and reply text:

```sql
SELECT table_name, column_name
FROM all_tab_columns
WHERE owner = 'USERS'
AND (
    UPPER(column_name) LIKE '%SURVEY_RESPONSE%'
    OR UPPER(column_name) LIKE '%QUESTION_SET_ITEM%'
    OR UPPER(column_name) LIKE '%REPLY%'
    OR UPPER(column_name) LIKE '%RESPONSE%'
)
ORDER BY table_name, column_id;
```

Look for columns similar to:

```text
SURVEY_RESPONSE_SK
QUESTION_SET_ITEM_SK
REPLY_TEXT
```

## 5. Query Saved Responses

After you identify the response table, query it by the `SurveyResponseSK`.

Replace `<response_table>` and `<survey_response_sk>`:

```sql
SELECT *
FROM <response_table>
WHERE survey_response_sk = <survey_response_sk>
ORDER BY question_set_item_sk;
```

Example:

```sql
SELECT *
FROM SURVEY_RESPONSE_ITEM
WHERE survey_response_sk = 948921
ORDER BY question_set_item_sk;
```

## 6. Check The Problem Field

For the future payroll issue, look for:

```text
3022 = EXPT_PAY_EE_FLG
3023 = EXPT_PAY_EE_TIME
```

Query only those items:

```sql
SELECT *
FROM <response_table>
WHERE survey_response_sk = <survey_response_sk>
AND question_set_item_sk IN (3022, 3023)
ORDER BY question_set_item_sk;
```

Expected value for `3023`:

```text
1
2
3
4
5
```

Wrong value:

```text
Within 30 days
30 to 90 days
6 months
One year
More than a year
```

If `3023` is saved as display text, backend cannot calculate the review date correctly.

## 7. Compare Against UI Request

In Visual Studio, put a breakpoint before:

```csharp
SavePortalResponsesAsync(saveResponseRequest)
```

Check:

```csharp
saveResponseRequest.Responses
```

For item `3023`, compare:

```text
UI request ReplyText
DB saved ReplyText
```

Result meaning:

```text
UI sends "Within 30 days" -> frontend mapping bug.
UI sends "1", DB saves "Within 30 days" -> backend/WCF transform issue.
UI sends "1", DB still has old "Within 30 days" -> frontend is not overwriting existing response.
UI sends "1", DB has "1", registration still fails -> another required field is missing.
```

## 8. Other Important Item Keys

Check these if final registration still fails:

```text
3140 = BUS_LGL_NAM
3146 = ER_EMAIL_ADR
3147 = EMAIL_NOTIFY
3160 = CNTC_FST_NAM
3161 = CNTC_LAST_NAM
```

Example:

```sql
SELECT *
FROM <response_table>
WHERE survey_response_sk = <survey_response_sk>
AND question_set_item_sk IN (3022, 3023, 3140, 3146, 3147, 3160, 3161)
ORDER BY question_set_item_sk;
```

## 9. What To Capture For The Bug

Capture these values:

```text
SurveyResponseSK
QuestionSetItemSK
ReplyText from UI request
ReplyText saved in DB
RegisterEmployerAsync rule violation text
```

This proves whether the fix belongs in frontend mapping, save/update flow, or backend/WCF processing.
