export function useStatusLabel() {
  function statusText(status) {
    const map = {
      Pending: "Chờ duyệt",
      Confirmed: "Đã xác nhận",
      Studying: "Đang học",
      Completed: "Hoàn thành",
      Cancelled: "Đã hủy",
      Passed: "Đạt",
      Failed: "Chưa đạt",
      InProgress: "Đang học",
      Paid: "Đã thanh toán",
      Partial: "Thanh toán một phần",
      Unpaid: "Chưa thanh toán",
      Overdue: "Quá hạn",
    };
    return map[status] || status || "-";
  }

  function statusClass(status) {
    const base = "px-2 py-0.5 text-xs font-bold rounded border ";
    if (["Studying", "Confirmed", "Paid", "Passed"].includes(String(status)))
      return base + "bg-emerald-50 text-emerald-600 border-emerald-200";
    if (["Pending", "Partial", "InProgress"].includes(String(status)))
      return base + "bg-blue-50 text-blue-600 border-blue-200";
    if (["Overdue", "Failed"].includes(String(status)))
      return base + "bg-red-50 text-red-600 border-red-200";
    return base + "bg-slate-100 text-slate-600 border-slate-200";
  }

  function shiftText(shift) {
    return (
      { Morning: "Ca Sáng", Afternoon: "Ca Chiều", Evening: "Ca Tối" }[shift] ||
      shift ||
      "Ca học"
    );
  }

  function dayText(day) {
    return (
      {
        Monday: "Thứ 2",
        Tuesday: "Thứ 3",
        Wednesday: "Thứ 4",
        Thursday: "Thứ 5",
        Friday: "Thứ 6",
        Saturday: "Thứ 7",
        Sunday: "Chủ nhật",
      }[day] ||
      day ||
      "Ngày học"
    );
  }

  return {
    statusText,
    statusClass,
    shiftText,
    dayText,
  };
}
