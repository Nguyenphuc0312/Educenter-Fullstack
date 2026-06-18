function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}

function slugify(value) {
  return String(value || 'educenter-export')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-zA-Z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .toLowerCase()
}

function todayStamp() {
  return new Date().toISOString().slice(0, 10)
}

function downloadBlob(content, filename, mimeType) {
  const blob = new Blob([content], { type: mimeType })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

function cellToCsv(value) {
  const text = String(value ?? '')
  return `"${text.replaceAll('"', '""')}"`
}

export function downloadCsv({ filename, columns, rows }) {
  const header = columns.map((column) => cellToCsv(column.label)).join(',')
  const body = rows
    .map((row) => columns.map((column) => cellToCsv(column.value(row))).join(','))
    .join('\n')
  downloadBlob(`\uFEFF${header}\n${body}`, filename, 'text/csv;charset=utf-8')
}

export function downloadIcs({ filename, events }) {
  const formatDate = (value) => {
    const date = value instanceof Date ? value : new Date(value)
    return date.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z$/, 'Z')
  }
  const lines = [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//EduCenter//Student Schedule//VI',
    'CALSCALE:GREGORIAN',
  ]

  events.forEach((event, index) => {
    lines.push(
      'BEGIN:VEVENT',
      `UID:${event.uid || `${Date.now()}-${index}@educenter.local`}`,
      `DTSTAMP:${formatDate(new Date())}`,
      `DTSTART:${formatDate(event.start)}`,
      `DTEND:${formatDate(event.end)}`,
      `SUMMARY:${String(event.title || '').replace(/\n/g, ' ')}`,
      `LOCATION:${String(event.location || '').replace(/\n/g, ' ')}`,
      `DESCRIPTION:${String(event.description || '').replace(/\n/g, ' ')}`,
      'END:VEVENT',
    )
  })

  lines.push('END:VCALENDAR')
  downloadBlob(lines.join('\r\n'), filename, 'text/calendar;charset=utf-8')
}

export function downloadExcelReport({
  title,
  subtitle,
  filename,
  user,
  summary = [],
  columns,
  rows,
  notes = [],
}) {
  const generatedAt = new Date()
  const tableRows = rows.length
    ? rows.map((row, index) => `
        <tr>
          <td class="index">${index + 1}</td>
          ${columns.map((column) => `<td>${escapeHtml(column.value(row))}</td>`).join('')}
        </tr>
      `).join('')
    : `<tr><td colspan="${columns.length + 1}" class="empty">Khong co du lieu</td></tr>`

  const html = `<!doctype html>
<html lang="vi">
<head>
  <meta charset="utf-8" />
  <meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8" />
  <title>${escapeHtml(title)}</title>
  <style>
    * { box-sizing: border-box; }
    body {
      margin: 0;
      padding: 32px;
      font-family: Inter, Arial, sans-serif;
      color: #0f172a;
      background: #eef4ff;
    }
    .document {
      max-width: 1120px;
      margin: 0 auto;
      background: #ffffff;
      border: 1px solid #dbeafe;
      border-radius: 18px;
      overflow: hidden;
      box-shadow: 0 24px 60px rgba(15, 23, 42, 0.12);
    }
    .hero {
      padding: 28px 32px;
      background: linear-gradient(135deg, #0f172a, #1d4ed8);
      color: #ffffff;
    }
    .brand {
      font-size: 13px;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: #93c5fd;
      margin-bottom: 12px;
    }
    h1 { margin: 0; font-size: 28px; line-height: 1.2; }
    .subtitle { margin: 8px 0 0; color: #dbeafe; font-size: 14px; }
    .meta {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 12px;
      padding: 18px 32px;
      background: #f8fafc;
      border-bottom: 1px solid #e2e8f0;
    }
    .meta-card {
      padding: 12px 14px;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      background: #ffffff;
    }
    .label { display: block; color: #64748b; font-size: 11px; font-weight: 800; text-transform: uppercase; letter-spacing: .05em; }
    .value { display: block; margin-top: 4px; font-weight: 800; color: #0f172a; }
    .content { padding: 28px 32px 32px; }
    .summary {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
      margin-bottom: 22px;
    }
    .summary-card {
      border: 1px solid #dbeafe;
      background: #eff6ff;
      border-radius: 14px;
      padding: 14px 16px;
    }
    .summary-card strong { display: block; margin-top: 4px; font-size: 20px; color: #1d4ed8; }
    table { width: 100%; border-collapse: separate; border-spacing: 0; overflow: hidden; border: 1px solid #e2e8f0; border-radius: 14px; }
    th { background: #0f172a; color: #ffffff; font-size: 12px; text-align: left; padding: 12px; }
    td { padding: 12px; border-top: 1px solid #e2e8f0; font-size: 13px; vertical-align: top; }
    tr:nth-child(even) td { background: #f8fafc; }
    .index { width: 52px; text-align: center; color: #64748b; font-weight: 700; }
    .empty { text-align: center; color: #64748b; padding: 32px; }
    .notes {
      margin-top: 22px;
      padding: 16px 18px;
      border-radius: 14px;
      background: #fffbeb;
      border: 1px solid #fde68a;
      color: #92400e;
      font-size: 13px;
    }
    .footer {
      margin-top: 26px;
      color: #64748b;
      font-size: 12px;
      display: flex;
      justify-content: space-between;
      gap: 16px;
    }
    @media print {
      body { background: #ffffff; padding: 0; }
      .document { box-shadow: none; border-radius: 0; border: 0; }
    }
  </style>
</head>
<body>
  <main class="document">
    <section class="hero">
      <div class="brand">EduCenter</div>
      <h1>${escapeHtml(title)}</h1>
      <p class="subtitle">${escapeHtml(subtitle || 'Bang Excel duoc tao tu du lieu hien tai tren he thong.')}</p>
    </section>
    <section class="meta">
      <div class="meta-card"><span class="label">Nguoi xuat</span><span class="value">${escapeHtml(user?.fullName || user?.username || 'Hoc vien')}</span></div>
      <div class="meta-card"><span class="label">Tai khoan</span><span class="value">${escapeHtml(user?.username || user?.email || '-')}</span></div>
      <div class="meta-card"><span class="label">Ngay xuat</span><span class="value">${generatedAt.toLocaleString('vi-VN')}</span></div>
    </section>
    <section class="content">
      ${summary.length ? `<div class="summary">${summary.map((item) => `
        <div class="summary-card"><span class="label">${escapeHtml(item.label)}</span><strong>${escapeHtml(item.value)}</strong></div>
      `).join('')}</div>` : ''}
      <table>
        <thead><tr><th>#</th>${columns.map((column) => `<th>${escapeHtml(column.label)}</th>`).join('')}</tr></thead>
        <tbody>${tableRows}</tbody>
      </table>
      ${notes.length ? `<div class="notes">${notes.map((note) => `<div>${escapeHtml(note)}</div>`).join('')}</div>` : ''}
      <div class="footer">
        <span>EduCenter - He thong quan ly trung tam dao tao</span>
        <span>${escapeHtml(filename)}</span>
      </div>
    </section>
  </main>
</body>
</html>`

  downloadBlob(html, filename, 'application/vnd.ms-excel;charset=utf-8')
}

export function downloadReportHtml(args) {
  return downloadExcelReport(args)
}

export function openPdfReport({
  title,
  subtitle,
  filename,
  user,
  summary = [],
  columns,
  rows,
  notes = [],
}) {
  const generatedAt = new Date()
  const tableRows = rows.length
    ? rows.map((row, index) => `
        <tr>
          <td class="index">${index + 1}</td>
          ${columns.map((column) => `<td>${escapeHtml(column.value(row))}</td>`).join('')}
        </tr>
      `).join('')
    : `<tr><td colspan="${columns.length + 1}" class="empty">Khong co du lieu</td></tr>`

  const html = `<!doctype html>
<html lang="vi">
<head>
  <meta charset="utf-8" />
  <title>${escapeHtml(title)}</title>
  <style>
    @page { size: A4; margin: 14mm; }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: Arial, "Helvetica Neue", sans-serif;
      color: #0f172a;
      background: #f8fafc;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }
    .sheet {
      max-width: 210mm;
      min-height: 297mm;
      margin: 0 auto;
      background: #fff;
      padding: 28px;
      border: 1px solid #e2e8f0;
    }
    .header {
      display: flex;
      justify-content: space-between;
      gap: 24px;
      padding-bottom: 18px;
      border-bottom: 3px solid #2563eb;
    }
    .brand { font-size: 13px; font-weight: 800; color: #2563eb; text-transform: uppercase; letter-spacing: .08em; }
    h1 { margin: 8px 0 0; font-size: 26px; line-height: 1.25; }
    .subtitle { margin: 8px 0 0; color: #475569; font-size: 13px; max-width: 620px; }
    .stamp { text-align: right; color: #475569; font-size: 12px; line-height: 1.6; }
    .meta, .summary {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 10px;
      margin-top: 18px;
    }
    .summary { grid-template-columns: repeat(auto-fit, minmax(145px, 1fr)); }
    .box {
      border: 1px solid #dbeafe;
      background: #eff6ff;
      border-radius: 8px;
      padding: 10px 12px;
    }
    .box.white { background: #fff; border-color: #e2e8f0; }
    .label { display: block; font-size: 10px; font-weight: 800; color: #64748b; text-transform: uppercase; letter-spacing: .05em; }
    .value { display: block; margin-top: 4px; font-size: 13px; font-weight: 800; color: #0f172a; word-break: break-word; }
    .summary .value { font-size: 18px; color: #1d4ed8; }
    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
      border: 1px solid #cbd5e1;
      font-size: 12px;
    }
    th {
      background: #0f172a;
      color: #fff;
      text-align: left;
      padding: 9px;
      border: 1px solid #0f172a;
    }
    td {
      padding: 9px;
      border: 1px solid #e2e8f0;
      vertical-align: top;
    }
    tr:nth-child(even) td { background: #f8fafc; }
    .index { width: 42px; text-align: center; color: #64748b; font-weight: 700; }
    .empty { text-align: center; color: #64748b; padding: 28px; }
    .notes {
      margin-top: 18px;
      padding: 12px 14px;
      border: 1px solid #fde68a;
      background: #fffbeb;
      color: #92400e;
      border-radius: 8px;
      font-size: 12px;
      line-height: 1.5;
    }
    .signatures {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 40px;
      margin-top: 36px;
      text-align: center;
      font-size: 12px;
      color: #334155;
    }
    .sign-space { height: 54px; }
    .footer {
      margin-top: 24px;
      display: flex;
      justify-content: space-between;
      color: #64748b;
      font-size: 10px;
      border-top: 1px solid #e2e8f0;
      padding-top: 10px;
    }
    .print-actions {
      position: sticky;
      top: 0;
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 10px;
      background: #f8fafc;
      border-bottom: 1px solid #e2e8f0;
    }
    .print-actions button {
      border: 0;
      border-radius: 8px;
      padding: 9px 14px;
      font-weight: 700;
      cursor: pointer;
    }
    .primary { background: #2563eb; color: #fff; }
    .secondary { background: #e2e8f0; color: #0f172a; }
    @media print {
      body { background: #fff; }
      .print-actions { display: none; }
      .sheet { border: 0; padding: 0; min-height: auto; }
    }
  </style>
</head>
<body>
  <div class="print-actions">
    <button class="secondary" onclick="window.close()">Dong</button>
    <button class="primary" onclick="window.print()">Luu PDF / In</button>
  </div>
  <main class="sheet">
    <section class="header">
      <div>
        <div class="brand">EduCenter</div>
        <h1>${escapeHtml(title)}</h1>
        <p class="subtitle">${escapeHtml(subtitle || 'Bao cao duoc tao tu he thong EduCenter.')}</p>
      </div>
      <div class="stamp">
        <strong>Ngay xuat</strong><br />
        ${generatedAt.toLocaleString('vi-VN')}<br />
        <span>${escapeHtml(filename || '')}</span>
      </div>
    </section>
    <section class="meta">
      <div class="box white"><span class="label">Nguoi xuat</span><span class="value">${escapeHtml(user?.fullName || user?.username || 'System')}</span></div>
      <div class="box white"><span class="label">Tai khoan</span><span class="value">${escapeHtml(user?.username || user?.email || '-')}</span></div>
      <div class="box white"><span class="label">Dinh dang</span><span class="value">PDF / Print</span></div>
    </section>
    ${summary.length ? `<section class="summary">${summary.map((item) => `
      <div class="box"><span class="label">${escapeHtml(item.label)}</span><span class="value">${escapeHtml(item.value)}</span></div>
    `).join('')}</section>` : ''}
    <table>
      <thead><tr><th>#</th>${columns.map((column) => `<th>${escapeHtml(column.label)}</th>`).join('')}</tr></thead>
      <tbody>${tableRows}</tbody>
    </table>
    ${notes.length ? `<section class="notes">${notes.map((note) => `<div>${escapeHtml(note)}</div>`).join('')}</section>` : ''}
    <section class="signatures">
      <div><strong>Nguoi lap</strong><div class="sign-space"></div><span>Ky, ghi ro ho ten</span></div>
      <div><strong>Xac nhan trung tam</strong><div class="sign-space"></div><span>Ky, ghi ro ho ten</span></div>
    </section>
    <section class="footer">
      <span>EduCenter - He thong quan ly trung tam dao tao</span>
      <span>${escapeHtml(filename || '')}</span>
    </section>
  </main>
  <script>
    window.addEventListener('load', () => setTimeout(() => window.print(), 350));
  </script>
</body>
</html>`

  const printWindow = window.open('', '_blank', 'width=980,height=720')
  if (!printWindow) {
    const fallbackName = String(filename || 'educenter-report.html').replace(/\.pdf$/i, '.html')
    downloadBlob(html, fallbackName, 'text/html;charset=utf-8')
    return
  }
  printWindow.document.open()
  printWindow.document.write(html)
  printWindow.document.close()
}

export function reportFilename(title, extension = 'xls') {
  return `${slugify(title)}-${todayStamp()}.${extension}`
}
