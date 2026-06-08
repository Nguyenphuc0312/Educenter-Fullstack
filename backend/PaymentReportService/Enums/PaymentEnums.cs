namespace PaymentReportService.Enums;

public enum UserRole { Admin = 1, Teacher = 2, Student = 3 }
public enum AccountStatus { Active = 1, Locked = 2 }
public enum InvoiceStatus { Unpaid = 1, Partial = 2, Paid = 3, Overdue = 4 }
public enum PaymentMethod { Cash = 1, BankTransfer = 2, Momo = 3, Vnpay = 4 }
public enum PaymentStatus { Success = 1, Pending = 2, Failed = 3, Cancelled = 4 }
