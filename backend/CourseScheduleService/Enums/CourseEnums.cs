namespace CourseScheduleService.Enums;

public enum CourseStatus { Draft = 0, Opening = 1, Closed = 2, ComingSoon = 3 }
public enum LearningMode { Offline = 0, Online = 1, Hybrid = 2 }
public enum ClassStatus { Open = 0, Full = 1, InProgress = 2, Completed = 3, Cancelled = 4 }
public enum StudyShift { Morning = 0, Afternoon = 1, Evening = 2 }
public enum ScheduleStatus { Scheduled = 0, Completed = 1, Cancelled = 2 }
public enum TeacherStatus { Active = 0, Inactive = 1 }
public enum SubstituteRequestStatus { Pending = 0, Approved = 1, Rejected = 2, Cancelled = 3 }
