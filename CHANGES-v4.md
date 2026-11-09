# Backend v4 - what changed

No database change, no migration.

## Filters
- `ClaimFilterDto` has a new `categoryId`. `GET /maker/claims` and `GET /approver/claims` now return only claims that contain an item of that category. The approver list already supported `employeeId`.
- Admin reports: `categoryId` now also works on `mismatched-claims` and `latest-claims` (claims that contain the category). `monthly-totals` already had it. With a category filter, `monthly-totals` leaves out employees with nothing in that category.

## Maker and approver names
- `ApproverClaimDto` (used by `/approver/claims` and `/admin/claims/{id}`) now has `employeeName` and `approverName`.
- `mismatched-claims` rows have `employeeName`. `latest-claims` rows have `approverId`, `approverName`, `decidedAt`.

## New endpoints
- `GET /admin/reports/items` and `GET /maker/claims/items` - every expense item behind a monthly total. Filters: `employeeId` (admin only, makers always get their own), `categoryId`, `from`, `to`, `status`, `index`, `limit`. Uses the same rows as `monthly-totals`, so the numbers always match. Each item has its claim id, status, maker name and approver name.
- `GET /lookup/employees` (Approver, Admin) - id + name list for the "filter by employee" picker.
