namespace StudentAttendanceService.Enums;

public enum StudentStatus { Active = 1, Inactive = 2, Suspended = 3 }
public enum Gender { Unknown = 0, Male = 1, Female = 2, Other = 3 }
public enum EnrollmentStatus { Pending = 1, Confirmed = 2, Studying = 3, Completed = 4, Cancelled = 5 }
public enum AttendanceSessionStatus { Open = 1, Locked = 2 }
public enum AttendanceStatus { Present = 1, Absent = 2, Late = 3, Excused = 4 }
public enum ResultStatus { InProgress = 1, Passed = 2, Failed = 3 }
