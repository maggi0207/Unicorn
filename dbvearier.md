# Export All Survey Responses From DBeaver

Use this after you find the parent survey row in `SRVY_RESP`.

Example from screenshot:

```text
SRVY_RESP_SK = 10006636
SRVY_NUM_TXT = 56613218 - Prior Registration
```

The next goal is to get every saved question/answer row for that `SRVY_RESP_SK`.

## 1. Find The Child Response Table

Run this first:

```sql
SELECT table_name, column_name
FROM all_tab_columns
WHERE owner = 'USERS'
AND UPPER(column_name) IN (
    'SRVY_RESP_SK',
    'QSTN_SET_ITEM_SK',
    'QUESTION_SET_ITEM_SK',
    'RPLY_TXT',
    'REPLY_TXT',
    'RSPNS_TXT',
    'RESPONSE_TXT'
)
ORDER BY table_name, column_id;
```

Find the table that has:

```text
SRVY_RESP_SK
QSTN_SET_ITEM_SK or QUESTION_SET_ITEM_SK
RPLY_TXT or REPLY_TXT
```

That table contains the answers.

## 2. Find Question Lookup Tables

Run this to find tables that describe question item keys:

```sql
SELECT table_name, column_name
FROM all_tab_columns
WHERE owner = 'USERS'
AND (
    UPPER(column_name) LIKE '%QSTN_SET_ITEM%'
    OR UPPER(column_name) LIKE '%QUESTION_SET_ITEM%'
    OR UPPER(column_name) LIKE '%QSTN%'
    OR UPPER(column_name) LIKE '%QUESTION%'
    OR UPPER(column_name) LIKE '%ITEM%'
)
ORDER BY table_name, column_id;
```

Look for a lookup table that has:

```text
QSTN_SET_ITEM_SK or QUESTION_SET_ITEM_SK
QSTN_TXT or QUESTION_TEXT
SHORT_DSC / DESCRIPTION / NAME
```

## 3. Export Raw Answers

Replace table and column names after you identify them.

If columns are abbreviated:

```sql
SELECT *
FROM <response_item_table>
WHERE srvy_resp_sk = 10006636
ORDER BY qstn_set_item_sk;
```

If columns use full names:

```sql
SELECT *
FROM <response_item_table>
WHERE survey_response_sk = 10006636
ORDER BY question_set_item_sk;
```

This gives the entire saved response from all steps.

## 4. Export Answers With Question Text

After finding the question lookup table, join it.

Abbreviated-column example:

```sql
SELECT
    r.srvy_resp_sk,
    r.qstn_set_item_sk,
    q.qstn_txt,
    r.rply_txt
FROM <response_item_table> r
LEFT JOIN <question_item_table> q
    ON q.qstn_set_item_sk = r.qstn_set_item_sk
WHERE r.srvy_resp_sk = 10006636
ORDER BY r.qstn_set_item_sk;
```

Full-column example:

```sql
SELECT
    r.survey_response_sk,
    r.question_set_item_sk,
    q.question_text,
    r.reply_text
FROM <response_item_table> r
LEFT JOIN <question_item_table> q
    ON q.question_set_item_sk = r.question_set_item_sk
WHERE r.survey_response_sk = 10006636
ORDER BY r.question_set_item_sk;
```

## 5. Check Critical Fields

Run this once you know the response item table:

```sql
SELECT *
FROM <response_item_table>
WHERE srvy_resp_sk = 10006636
AND qstn_set_item_sk IN (
    3022,
    3023,
    3140,
    3146,
    3147,
    3160,
    3161
)
ORDER BY qstn_set_item_sk;
```

Important fields:

```text
3022 = EXPT_PAY_EE_FLG
3023 = EXPT_PAY_EE_TIME
3140 = BUS_LGL_NAM
3146 = ER_EMAIL_ADR
3147 = EMAIL_NOTIFY
3160 = CNTC_FST_NAM
3161 = CNTC_LAST_NAM
```

Expected for `3023`:

```text
1
2
3
4
5
```

Wrong for `3023`:

```text
Within 30 days
30 to 90 days
6 months
One year
More than a year
```

## 6. Export Results From DBeaver

In the result grid:

1. Right-click the result grid.
2. Choose `Export Data`.
3. Choose `CSV`.
4. Export the full result.
5. Keep the columns for item key, question text, and reply text.

## 7. Compare With Frontend Model

In Visual Studio, compare DB export to:

```csharp
model.GetSurveyResponses()
```

and:

```csharp
saveResponseRequest.Responses
```

Decision:

```text
Exists in model, missing in request -> mapping dropped it.
Exists in request, missing in DB -> save/update issue.
Exists in DB with display text -> bad frontend mapping or old value not overwritten.
Exists in DB with correct value, registration fails -> another required field is missing.
```

## 8. What To Send Back For Analysis

Share these from the DB export:

```text
SRVY_RESP_SK
QSTN_SET_ITEM_SK
Question text or description
Reply text
Final RegisterEmployerAsync rule violation text
```

Start with the critical fields in section 5. If those look correct, export all rows and compare the full response.
