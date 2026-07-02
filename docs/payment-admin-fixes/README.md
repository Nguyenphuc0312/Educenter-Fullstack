# Payment Admin Fixes

Branch: `payment-admin-fixes`

## Scope

This branch contains two small fixes for the Payment & Report area:

- Fix admin table status filters when API data returns enum names such as `Partial`, `Paid`, `Overdue`, or `Pending`.
- Sort payment transactions so pending payment requests appear first, with the newest request at the top.

## Changed Files

- `frontend/src/components/admin/AdminResourceView.vue`
- `backend/PaymentReportService/Services/PaymentServices.cs`

## Behavior

### Tuition status filter

Admin status filters now normalize both numeric enum values and string enum names before comparing. This prevents empty tables when filtering statuses such as overdue, paid, or partial invoices.

### Payment request ordering

The payment transaction list is ordered by:

1. Pending transactions first.
2. Student-created pending requests before admin-created pending demo records.
3. Newest `CreatedAt` first.
4. Newest `PaymentDate` first as a fallback.

This helps admins review the newest student payment requests more easily.

## Impact

- No database schema change.
- No API contract change.
- No change to payment confirmation logic.
- No change to Course & Schedule Service.
- No change to Student & Attendance Service.

## Verification

Build checked:

```bash
dotnet build backend/PaymentReportService/PaymentReportService.csproj
```

Expected UI check:

- Open Admin > Hoc phi.
- Filter status by Qua han, Mot phan, or Da thanh toan.
- The table should show matching rows instead of becoming empty incorrectly.
- Open Admin > Thanh toan.
- Pending payment requests should appear at the top, newest first.
