<template>
  <div class="space-y-6">
    <PageHeader
      title="Học phí & Hóa đơn"
      subtitle="Quản lý giao dịch, thanh toán công nợ và tra cứu biên lai điện tử."
    >
      <template #actions>
        <button
          class="student-btn-secondary disabled:opacity-70 disabled:cursor-not-allowed"
          @click="refreshData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-current" />
          <svg
            v-else
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
            />
          </svg>
          Làm mới
        </button>

        <button
          class="student-btn-primary disabled:opacity-70 disabled:cursor-not-allowed"
          @click="exportReport"
          :disabled="isExporting || loading"
        >
          <LoadingSpinner v-if="isExporting" size="sm" class="!text-white" />
          <svg
            v-else
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
          Xuất báo cáo
        </button>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex flex-col justify-center transition-all hover:border-slate-300">
        <span class="text-slate-500 text-sm font-semibold uppercase tracking-wider mb-1">Tổng học phí đã phát sinh</span>
        <strong class="text-2xl text-slate-800 font-bold">{{ formatVnd(summary.total) }}</strong>
      </div>
      <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex flex-col justify-center transition-all hover:border-emerald-200">
        <span class="text-emerald-600 text-sm font-semibold uppercase tracking-wider mb-1">Đã thanh toán</span>
        <strong class="text-2xl text-emerald-700 font-bold">{{ formatVnd(summary.paid) }}</strong>
      </div>
      <div class="bg-red-50 rounded-xl p-5 border border-red-100 shadow-sm flex flex-col justify-center relative overflow-hidden transition-all hover:shadow-md">
        <div class="absolute right-0 top-0 bottom-0 w-24 bg-gradient-to-l from-red-100 to-transparent"></div>
        <span class="text-red-600 text-sm font-semibold uppercase tracking-wider mb-1 relative z-10">Tổng công nợ hiện tại</span>
        <strong class="text-3xl text-red-600 font-black relative z-10">{{ formatVnd(summary.debt) }}</strong>
      </div>
    </div>

    <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      <div class="border-b border-slate-200 px-6 pt-4">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="all" tab="Tất cả hóa đơn" />
          <a-tab-pane key="unpaid" tab="Cần thanh toán" />
          <a-tab-pane key="paid" tab="Đã hoàn tất" />
        </a-tabs>
      </div>

      <div class="p-0">
        <a-table
          :data-source="filteredInvoices"
          :columns="columns"
          :loading="loading"
          row-key="id"
          :pagination="{ pageSize: 10, showSizeChanger: true, showTotal: (total) => `Tổng số ${total} hóa đơn` }"
          :scroll="{ x: 1000 }"
          class="enterprise-table"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'invoiceInfo'">
              <div class="font-medium text-slate-800">{{ record.invoiceCode }}</div>
              <div class="text-xs text-slate-500 mt-0.5">
                Hạn: <span :class="isOverdue(record.dueDate, record.status) ? 'text-red-500 font-bold' : ''">{{ formatDate(record.dueDate) }}</span>
              </div>
            </template>

            <template v-else-if="column.key === 'courseInfo'">
              <div class="font-semibold text-slate-800 line-clamp-1" :title="record.courseNameSnapshot">
                {{ record.courseNameSnapshot }}
              </div>
              <div class="text-sm text-slate-500">{{ record.classNameSnapshot }}</div>
            </template>

            <template v-else-if="column.key === 'amounts'">
              <div class="flex justify-between text-sm mb-1">
                <span class="text-slate-500">Tổng:</span>
                <span class="font-medium text-slate-800">{{ formatVnd(record.totalAmount) }}</span>
              </div>
              <div class="flex justify-between text-sm border-t border-slate-100 pt-1">
                <span class="text-slate-500">Còn nợ:</span>
                <span :class="record.debtAmount > 0 ? 'font-bold text-red-600' : 'font-medium text-emerald-600'">
                  {{ formatVnd(record.debtAmount) }}
                </span>
              </div>
            </template>

            <template v-else-if="column.key === 'status'">
              <span :class="statusBadge(record.status, record.dueDate)">
                {{ statusText(record.status, record.dueDate) }}
              </span>
            </template>

            <template v-else-if="column.key === 'actions'">
              <div class="flex flex-nowrap items-center gap-2 justify-end min-w-max">
                <button
                  v-if="record.debtAmount > 0"
                  class="px-3 py-1.5 text-white text-xs font-semibold rounded transition-colors shadow-sm whitespace-nowrap disabled:cursor-not-allowed disabled:shadow-none"
                  :class="hasPendingPayment(record.id) ? 'bg-amber-500' : 'bg-blue-600 hover:bg-blue-700 active:scale-95'"
                  :disabled="hasPendingPayment(record.id)"
                  @click="openPaymentModal(record)"
                >
                  {{ hasPendingPayment(record.id) ? 'Chờ xác minh' : 'Thanh toán' }}
                </button>
                <button
                  v-if="record.paidAmount > 0"
                  class="px-3 py-1.5 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-medium rounded transition-colors whitespace-nowrap active:scale-95 flex items-center gap-1.5 disabled:opacity-70 disabled:cursor-not-allowed"
                  @click="downloadReceipt(record)"
                  :disabled="record.isDownloading"
                >
                  <LoadingSpinner v-if="record.isDownloading" size="xs" class="text-slate-500" />
                  <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
                  Tải biên lai
                </button>
              </div>
            </template>
          </template>
          <template #emptyText>
             <a-empty description="Không tìm thấy hóa đơn nào." />
          </template>
        </a-table>
      </div>
    </section>

    <a-modal
      v-model:open="isPaymentModalVisible"
      title="Thanh toán hóa đơn dịch vụ"
      centered
      width="500px"
      :footer="null"
      destroyOnClose
    >
      <div v-if="selectedInvoice" class="py-2">
        <div v-if="paymentStep === 1">
          <div class="bg-slate-50 p-4 rounded-xl border border-slate-200 mb-6">
            <div class="flex justify-between items-center mb-3 pb-3 border-b border-slate-200">
              <span class="text-slate-500">Mã giao dịch</span>
              <strong class="text-slate-800 font-mono">{{ selectedInvoice.invoiceCode }}</strong>
            </div>
            <div class="flex justify-between items-start mb-2">
              <span class="text-slate-500 w-1/3">Nội dung</span>
              <span class="text-slate-800 font-medium text-right w-2/3">{{ selectedInvoice.courseNameSnapshot }} - {{ selectedInvoice.classNameSnapshot }}</span>
            </div>
            <div class="flex justify-between items-center mt-4 pt-3 border-t border-slate-200">
              <span class="text-base font-semibold text-slate-800">Số tiền cần thanh toán</span>
              <span class="text-2xl font-black text-blue-600">{{ formatVnd(paymentAmount) }}</span>
            </div>
          </div>

          <h3 class="font-bold text-slate-800 mb-3">Mức thanh toán</h3>
          <div v-if="canChoosePartial" class="grid grid-cols-2 gap-3 mb-6">
            <button type="button" class="py-3 rounded-lg border text-sm font-bold transition-colors" :class="paymentPercent === 50 ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-slate-200 bg-white text-slate-700 hover:border-blue-300'" @click="paymentPercent = 50">
              Thanh toán 50%
            </button>
            <button type="button" class="py-3 rounded-lg border text-sm font-bold transition-colors" :class="paymentPercent === 100 ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-slate-200 bg-white text-slate-700 hover:border-blue-300'" @click="paymentPercent = 100">
              Thanh toán toàn bộ
            </button>
          </div>
          <div v-else class="mb-6 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            Hóa đơn đã thanh toán một phần. Lần tiếp theo cần thanh toán toàn bộ số tiền còn lại.
          </div>

          <h3 class="font-bold text-slate-800 mb-3">Phương thức</h3>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-4">
            <button
              v-for="method in paymentMethods"
              :key="method.value"
              type="button"
              class="rounded-lg border px-3 py-3 text-left transition-colors"
              :class="selectedPaymentMethod === method.value ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-slate-200 bg-white text-slate-700 hover:border-blue-300'"
              @click="selectedPaymentMethod = method.value"
            >
              <div class="text-sm font-bold">{{ method.label }}</div>
              <div class="mt-1 text-xs opacity-80">{{ method.short }}</div>
            </button>
          </div>
          <div class="mb-6 rounded-lg border border-blue-200 bg-blue-50 p-4">
            <div class="font-semibold text-slate-800">{{ selectedMethodInfo.label }}</div>
            <div class="mt-1 text-sm text-slate-600">{{ selectedMethodInfo.description }}</div>
          </div>

          <div class="flex gap-3">
            <button
              class="flex-1 py-2.5 bg-white border border-slate-300 text-slate-700 font-semibold rounded-lg hover:bg-slate-50 transition-colors"
              @click="isPaymentModalVisible = false"
            >
              Hủy bỏ
            </button>
            <button
              class="flex-1 py-2.5 bg-blue-600 text-white font-bold rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-70 disabled:cursor-not-allowed flex justify-center items-center gap-2"
              :disabled="isProcessing"
              @click="generateQR"
            >
              <LoadingSpinner v-if="isProcessing" size="sm" class="!text-white" />
              <span v-else>Xem hướng dẫn thanh toán</span>
            </button>
          </div>
        </div>

        <div v-else-if="paymentStep === 2" class="text-center py-4">
          <h3 class="text-lg font-bold text-slate-800 mb-2">{{ selectedMethodInfo.instructionTitle }}</h3>
          <p class="text-sm text-slate-500 mb-6">
            Thanh toán đúng <strong class="text-slate-700">{{ formatVnd(paymentAmount) }}</strong> với nội dung <strong class="text-slate-700">{{ transferContent }}</strong>.
          </p>

          <div v-if="activeQrUrl && !qrImageFailed" class="bg-white p-4 rounded-lg border border-blue-200 inline-block mb-6">
            <img :src="activeQrUrl" :alt="selectedMethodInfo.qrAlt" class="w-48 h-48 mx-auto object-contain" @error="qrImageFailed = true" />
          </div>
          <div v-else class="mb-6 rounded-lg border border-slate-200 bg-slate-50 px-4 py-4 text-left text-sm text-slate-600">
            QR cho ph??ng th?c n?y ch?a ???c c?u h?nh ho?c file ?nh ch?a t?n t?i. H?y ??t ?nh t?i <strong>frontend/public/payment-qr/techcombank-qr.jpg</strong> ho?c c?u h?nh VietQR ??ng.
          </div>

          <div class="bg-amber-50 border border-amber-200 p-3 rounded-lg text-sm text-amber-700 text-left mb-6">
            <p class="font-medium">Lưu ý quan trọng:</p>
            <ul class="list-disc pl-5 mt-1 space-y-1">
              <li>Không thay đổi nội dung thanh toán.</li>
              <li>Gửi yêu cầu không đồng nghĩa với thanh toán thành công.</li>
              <li>Công nợ chỉ cập nhật sau khi quản trị viên xác nhận tiền đã vào tài khoản.</li>
            </ul>
          </div>

          <div class="flex gap-3">
            <button
              class="flex-1 py-2.5 bg-white border border-slate-300 text-slate-700 font-semibold rounded-lg hover:bg-slate-50 transition-colors"
              @click="paymentStep = 1"
            >
              Quay lại
            </button>
            <button
              class="flex-1 py-2.5 bg-emerald-600 text-white font-bold rounded-lg hover:bg-emerald-700 transition-colors flex justify-center items-center disabled:opacity-60 disabled:cursor-not-allowed"
              :disabled="isSubmittingPayment"
              @click="submitPaymentRequest"
            >
              <LoadingSpinner v-if="isSubmittingPayment" size="sm" class="!text-white" />
              <span v-else>Đã thanh toán, gửi xác minh</span>
            </button>
          </div>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { message } from "ant-design-vue";
import dayjs from "dayjs";
import PageHeader from "@/components/ui/PageHeader.vue";
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";
import { tuitionApi } from "@/api/tuitionApi";
import { paymentApi } from "@/api/paymentApi";
import { useAuthStore } from "@/stores/auth";
import { formatDate, formatVnd } from "@/lib/formatters";
import { downloadExcelReport, reportFilename } from "@/lib/exportDocuments";

const auth = useAuthStore();
const invoices = ref([]);
const payments = ref([]);
const loading = ref(false);
const error = ref("");
const isExporting = ref(false);

// Table State
const activeTab = ref("all");
const columns = [
  { title: "Hóa đơn", key: "invoiceInfo", width: 160 },
  { title: "Nội dung đào tạo", key: "courseInfo" },
  { title: "Giá trị / Công nợ", key: "amounts", width: 220 },
  { title: "Trạng thái", key: "status", width: 150 },
  { title: "Thao tác", key: "actions", align: "right", width: 200 },
];

// Modal State
const isPaymentModalVisible = ref(false);
const selectedInvoice = ref(null);
const paymentPercent = ref(100);
const selectedPaymentMethod = ref(2);
const isProcessing = ref(false);
const paymentStep = ref(1);
const isSubmittingPayment = ref(false);
const qrImageFailed = ref(false);
const defaultQrUrl = "/payment-qr/techcombank-qr.jpg";
const staticBankQrUrl = import.meta.env.VITE_BANK_QR_URL || import.meta.env.VITE_PAYMENT_QR_URL || defaultQrUrl;
const momoQrUrl = import.meta.env.VITE_MOMO_QR_URL || staticBankQrUrl;
const vnpayQrUrl = import.meta.env.VITE_VNPAY_QR_URL || "";
const vietQrBankId = import.meta.env.VITE_VIETQR_BANK_ID || "";
const vietQrAccountNo = import.meta.env.VITE_VIETQR_ACCOUNT_NO || "";
const vietQrAccountName = import.meta.env.VITE_VIETQR_ACCOUNT_NAME || "";

const paymentMethods = [
  {
    value: 2,
    label: "Chuyển khoản",
    short: "Ngân hàng/VietQR",
    description: "Quét QR ngân hàng hoặc chuyển khoản đúng nội dung để quản trị viên đối soát.",
    instructionTitle: "Thông tin chuyển khoản",
    qrAlt: "Mã QR chuyển khoản ngân hàng",
  },
  {
    value: 3,
    label: "Momo",
    short: "Ví điện tử mock",
    description: "Mô phỏng thanh toán qua ví Momo. Sau khi thanh toán, gửi yêu cầu để admin xác minh.",
    instructionTitle: "Thông tin thanh toán Momo",
    qrAlt: "Mã QR Momo",
  },
  {
    value: 4,
    label: "VNPay",
    short: "Cổng online mock",
    description: "Mô phỏng thanh toán VNPay QR có số tiền và nội dung hóa đơn cho demo.",
    instructionTitle: "Thông tin thanh toán VNPay",
    qrAlt: "Mã QR VNPay",
  },
];

const canChoosePartial = computed(() => Number(selectedInvoice.value?.paidAmount || 0) <= 0);
const paymentAmount = computed(() => {
  const debt = Number(selectedInvoice.value?.debtAmount || 0);
  const total = Number(selectedInvoice.value?.totalAmount || 0);
  if (!canChoosePartial.value) return debt;
  return paymentPercent.value === 50 ? Math.min(Math.round(total * 0.5), debt) : debt;
});
const transferContent = computed(() => `EDU ${selectedInvoice.value?.invoiceCode || ""}`.trim());
const selectedMethodInfo = computed(() => paymentMethods.find((item) => item.value === selectedPaymentMethod.value) || paymentMethods[0]);
const dynamicVietQrUrl = computed(() => {
  if (vietQrBankId && vietQrAccountNo) {
    const params = new URLSearchParams({
      amount: String(paymentAmount.value || 0),
      addInfo: transferContent.value,
      accountName: vietQrAccountName,
    });
    return `https://img.vietqr.io/image/${vietQrBankId}-${vietQrAccountNo}-compact2.png?${params.toString()}`;
  }
  return "";
});
const activeQrUrl = computed(() => {
  if (selectedPaymentMethod.value === 2) return dynamicVietQrUrl.value || staticBankQrUrl;
  if (selectedPaymentMethod.value === 3) return momoQrUrl || dynamicVietQrUrl.value || staticBankQrUrl;
  if (selectedPaymentMethod.value === 4) return vnpayQrUrl || dynamicVietQrUrl.value || staticBankQrUrl;
  return staticBankQrUrl;
});
const pendingInvoiceIds = computed(() => new Set(
  payments.value
    .filter((item) => item.status === 2 || item.status === "2" || item.status === "Pending")
    .map((item) => item.invoiceId),
));

// Lọc hóa đơn theo tab
const filteredInvoices = computed(() => {
  if (activeTab.value === "unpaid") return invoices.value.filter((i) => i.debtAmount > 0);
  if (activeTab.value === "paid") return invoices.value.filter((i) => i.debtAmount === 0);
  return invoices.value;
});

// Tóm tắt công nợ
const summary = computed(() =>
  invoices.value.reduce(
    (sum, item) => ({
      total: sum.total + Number(item.totalAmount || 0),
      paid: sum.paid + Number(item.paidAmount || 0),
      debt: sum.debt + Number(item.debtAmount || 0),
    }),
    { total: 0, paid: 0, debt: 0 },
  ),
);

onMounted(() => loadData());

// Lấy dữ liệu
async function loadData() {
  loading.value = true;
  error.value = "";
  try {
    if (!auth.user?.referenceId) {
      invoices.value = [];
      payments.value = [];
      return;
    }
    const [invoiceData, paymentData] = await Promise.all([
      tuitionApi.getByStudent(auth.user.referenceId),
      paymentApi.getByStudent(auth.user.referenceId),
    ]);
    invoices.value = invoiceData;
    payments.value = paymentData;
  } catch (err) {
    error.value = err.message || "Hệ thống không thể kết nối dữ liệu tài chính lúc này.";
  } finally {
    loading.value = false;
  }
}

// Nút "Làm mới"
async function refreshData() {
  await loadData();
  message.success("Dữ liệu đã được làm mới!");
}

// Nút "Xuất báo cáo"
async function exportReport() {
  if (filteredInvoices.value.length === 0) {
    message.warning("Không có dữ liệu hóa đơn trong danh sách để xuất báo cáo.");
    return;
  }

  isExporting.value = true;
  message.loading({ content: "Đang tạo file báo cáo học phí...", key: "exportReport" });

  try {
    downloadExcelReport({
      title: "Báo cáo học phí & hóa đơn",
      subtitle: "Tổng hợp công nợ, thanh toán và trạng thái hóa đơn của học viên.",
      filename: reportFilename("bao-cao-hoc-phi-hoa-don"),
      user: auth.user,
      summary: [
        { label: "Tổng học phí", value: formatVnd(summary.value.total) },
        { label: "Đã thanh toán", value: formatVnd(summary.value.paid) },
        { label: "Còn nợ", value: formatVnd(summary.value.debt) },
        { label: "Số hóa đơn", value: filteredInvoices.value.length },
      ],
      columns: [
        { label: "Mã hóa đơn", value: (x) => x.invoiceCode },
        { label: "Khóa học", value: (x) => x.courseNameSnapshot },
        { label: "Lớp", value: (x) => x.classNameSnapshot },
        { label: "Hạn thanh toán", value: (x) => formatDate(x.dueDate) },
        { label: "Tổng tiền", value: (x) => formatVnd(x.totalAmount) },
        { label: "Đã trả", value: (x) => formatVnd(x.paidAmount) },
        { label: "Còn nợ", value: (x) => formatVnd(x.debtAmount) },
        { label: "Trạng thái", value: (x) => statusText(x.status, x.dueDate) },
      ],
      rows: filteredInvoices.value,
      notes: [
        "File này được tạo từ dữ liệu hóa đơn đang hiển thị trên hệ thống.",
        "Có thể mở file bằng Excel hoặc LibreOffice để lọc, thống kê và in khi cần nộp hoặc lưu trữ.",
      ],
    });
    message.success({ content: "Xuất báo cáo học phí thành công.", key: "exportReport", duration: 3 });
  } catch (err) {
    message.error({ content: "Đã xảy ra lỗi khi tạo báo cáo. Vui lòng thử lại sau.", key: "exportReport", duration: 3 });
  } finally {
    isExporting.value = false;
  }
}

function hasPendingPayment(invoiceId) {
  return pendingInvoiceIds.value.has(invoiceId);
}

function latestSuccessfulPayment(invoiceId) {
  return [...payments.value]
    .filter((item) => item.invoiceId === invoiceId && (item.status === 1 || item.status === "1" || item.status === "Success"))
    .sort((a, b) => new Date(b.paymentDate || b.createdAt || 0) - new Date(a.paymentDate || a.createdAt || 0))[0] || null;
}

function paymentMethodText(method) {
  const value = Number(method);
  return ({ 1: "Chuyển khoản", 2: "Chuyển khoản", 3: "Momo", 4: "VNPay" }[value] || method || "Theo giao dịch hệ thống");
}

// Nút "Tải biên lai"
async function downloadReceipt(invoice) {
  invoice.isDownloading = true;
  message.loading({ content: `Đang tạo biên lai cho ${invoice.invoiceCode}...`, key: `pdf-${invoice.id}` });

  try {
    const successPayment = latestSuccessfulPayment(invoice.id);
    downloadExcelReport({
      title: "Biên lai / Hóa đơn học phí",
      subtitle: "Chứng từ thanh toán điện tử tạo từ EduCenter.",
      filename: reportFilename(`bien-lai-${invoice.invoiceCode || invoice.id}`),
      user: auth.user,
      summary: [
        { label: "Mã hóa đơn", value: invoice.invoiceCode || invoice.id },
        { label: "Mã giao dịch", value: successPayment?.id || "-" },
        { label: "Trạng thái", value: statusText(invoice.status, invoice.dueDate) },
        { label: "Đã thanh toán", value: formatVnd(invoice.paidAmount) },
        { label: "Còn nợ", value: formatVnd(invoice.debtAmount) },
      ],
      columns: [
        { label: "Nội dung", value: (x) => x.label },
        { label: "Thông tin", value: (x) => x.value },
      ],
      rows: [
        { label: "Học viên", value: auth.user?.fullName || auth.user?.username || "-" },
        { label: "Khóa học", value: invoice.courseNameSnapshot || "-" },
        { label: "Lớp", value: invoice.classNameSnapshot || "-" },
        { label: "Ngày hạn", value: formatDate(invoice.dueDate) },
        { label: "Tổng tiền", value: formatVnd(invoice.totalAmount) },
        { label: "Đã thanh toán", value: formatVnd(invoice.paidAmount) },
        { label: "Còn nợ", value: formatVnd(invoice.debtAmount) },
        { label: "Giao dịch gần nhất", value: successPayment ? `${formatVnd(successPayment.amount)} - ${formatDate(successPayment.paymentDate)}` : "-" },
        { label: "Phương thức", value: paymentMethodText(successPayment?.method) },
      ],
      notes: [
        "File Excel này có thể mở bằng Excel hoặc LibreOffice để lọc, thống kê và in khi cần.",
        "Thông tin được tạo từ dữ liệu hóa đơn hiện có của học viên.",
      ],
    });
    message.success({ content: `Đã tải biên lai Excel: ${invoice.invoiceCode}.xls`, key: `pdf-${invoice.id}`, duration: 2 });
  } catch (err) {
    message.error({ content: `Không thể tải biên lai cho hóa đơn ${invoice.invoiceCode}.`, key: `pdf-${invoice.id}`, duration: 2 });
  } finally {
    invoice.isDownloading = false;
  }
}
// Thanh toán - Hiển thị Modal
function openPaymentModal(invoice) {
  selectedInvoice.value = invoice;
  paymentPercent.value = 100;
  selectedPaymentMethod.value = 2;
  paymentStep.value = 1;
  qrImageFailed.value = false;
  isPaymentModalVisible.value = true;
}

// Thanh toán - Tạo mã QR
async function generateQR() {
  isProcessing.value = true;
  try {
    await new Promise((resolve) => setTimeout(resolve, 800));
    paymentStep.value = 2;
  } catch (error) {
    message.error("Không thể tạo mã QR. Vui lòng kiểm tra lại kết nối.");
  } finally {
    isProcessing.value = false;
  }
}

async function submitPaymentRequest() {
  if (!selectedInvoice.value) return;
  isSubmittingPayment.value = true;
  try {
    await paymentApi.studentRequest({
      invoiceId: selectedInvoice.value.id,
      percent: canChoosePartial.value ? paymentPercent.value : 100,
      method: selectedPaymentMethod.value,
      note: `Học viên báo đã thanh toán qua ${selectedMethodInfo.value.label}. Nội dung: ${transferContent.value}`,
    });
    message.success("Đã gửi yêu cầu xác minh thanh toán");
    isPaymentModalVisible.value = false;
    await loadData();
  } catch (error) {
    message.error(error.message || "Không thể gửi yêu cầu xác minh thanh toán");
  } finally {
    isSubmittingPayment.value = false;
  }
}

// Helpers giao diện
function isOverdue(dueDate, status) {
  if (status === "Paid") return false;
  return dayjs(dueDate).isBefore(dayjs(), "day");
}

function statusText(status, dueDate) {
  if (status !== "Paid" && isOverdue(dueDate, status)) return "Quá hạn";
  return ({ Paid: "Đã hoàn tất", Partial: "Thanh toán 1 phần", Unpaid: "Chưa thanh toán" }[status] || status);
}

function statusBadge(status, dueDate) {
  const base = "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ";
  if (status === "Paid") return base + "bg-emerald-50 text-emerald-700 border-emerald-200";
  if (status !== "Paid" && isOverdue(dueDate, status)) return base + "bg-red-50 text-red-700 border-red-200";
  if (status === "Partial") return base + "bg-amber-50 text-amber-700 border-amber-200";
  return base + "bg-slate-50 text-slate-700 border-slate-300";
}
</script>

<style scoped>
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  padding: 12px 16px;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 16px;
  vertical-align: top;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
</style>
