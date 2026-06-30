<template>
  <div class="space-y-6">
    <PageHeader 
      title="Nháº­p káº¿t quáº£ há»c táº­p" 
      subtitle="ÄÃ¡nh giÃ¡ Ä‘iá»ƒm thi, chuyÃªn cáº§n vÃ  káº¿t xuáº¥t báº£ng Ä‘iá»ƒm cho há»c viÃªn."
    >
      <template #actions>
        <div class="hidden lg:flex items-center bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden mr-2">
          <a-tooltip title="Táº£i file máº«u (CSV)">
            <button @click="downloadTemplate" class="px-3 py-2 text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors border-r border-slate-200">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </button>
          </a-tooltip>
          <button @click="triggerImport" class="px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors border-r border-slate-200 flex items-center gap-1.5" :disabled="!selectedClassId">
            <svg class="w-4 h-4 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" /></svg>
            Nháº­p CSV
          </button>
          <input type="file" ref="fileInput" accept=".csv" class="hidden" @change="handleImportCSV" />
          <button @click="exportToCSV" class="px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors flex items-center gap-1.5" :disabled="!rows.length">
            <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
            Xuáº¥t CSV
          </button>
        </div>

        <button 
          class="px-4 py-2 bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 active:scale-95" 
          @click="loadBaseData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-slate-500" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          LÃ m má»›i
        </button>
        <button 
          class="px-5 py-2 font-bold rounded-lg shadow-sm transition-all flex items-center gap-2 disabled:opacity-50 active:scale-95"
          :class="isDirty ? 'bg-amber-500 hover:bg-amber-600 text-white animate-pulse-fast' : 'bg-blue-600 hover:bg-blue-700 text-white'"
          :disabled="!rows.length || saving" 
          @click="saveAll"
        >
          <LoadingSpinner v-if="saving" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" /></svg>
          {{ isDirty ? 'LÆ°u thay Ä‘á»•i (*)' : 'LÆ°u báº£ng Ä‘iá»ƒm' }}
        </button>
      </template>
    </PageHeader>

    <section class="bg-white rounded-2xl p-5 border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div class="flex-1 max-w-xl">
        <label class="text-xs font-bold text-slate-600 uppercase tracking-wider mb-2 flex items-center gap-1.5">
          <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" /></svg>
          Chá»n Lá»›p há»c
        </label>
        <a-select 
          v-model:value="selectedClassId" 
          placeholder="TÃ¬m kiáº¿m vÃ  chá»n lá»›p há»c..." 
          class="w-full custom-select" 
          @change="loadClassData"
          show-search
          option-filter-prop="children"
        >
          <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
            <div class="font-medium text-slate-700">
              <span class="font-bold text-blue-600 mr-2">[{{ cls.classCode || 'Lá»šP' }}]</span> {{ cls.className }}
            </div>
          </a-select-option>
        </a-select>
      </div>

      <div v-if="isDirty" class="flex items-center gap-3 bg-amber-50 border border-amber-200 px-4 py-2.5 rounded-xl text-amber-700 text-xs font-medium md:max-w-xs animate-fade-in">
        <svg class="w-5 h-5 shrink-0 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
        <p>PhÃ¡t hiá»‡n thay Ä‘á»•i chÆ°a Ä‘Æ°á»£c lÆ°u. HÃ£y nháº¥n <strong>LÆ°u thay Ä‘á»•i</strong> trÆ°á»›c khi thoÃ¡t!</p>
      </div>
    </section>

    <div v-if="loading" class="py-24 flex justify-center">
      <LoadingSpinner size="lg" />
    </div>

    <div v-else-if="!selectedClassId" class="py-20 bg-slate-50/50 rounded-2xl border border-dashed border-slate-300 text-center flex flex-col items-center">
      <div class="w-20 h-20 bg-white rounded-full shadow-sm border border-slate-100 flex items-center justify-center text-slate-300 mb-5">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" /></svg>
      </div>
      <h2 class="text-xl font-black text-slate-700 mb-2">ChÆ°a chá»n lá»›p há»c</h2>
      <p class="text-slate-500 max-w-md mx-auto">Sá»­ dá»¥ng há»™p chá»n phÃ­a trÃªn Ä‘á»ƒ báº¯t Ä‘áº§u phiÃªn nháº­p Ä‘iá»ƒm.</p>
    </div>

    <div v-else-if="rows.length === 0" class="py-20 bg-slate-50/50 rounded-2xl border border-dashed border-slate-300 text-center flex flex-col items-center">
      <div class="w-20 h-20 bg-white rounded-full shadow-sm border border-slate-100 flex items-center justify-center text-slate-300 mb-5">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
      </div>
      <h2 class="text-xl font-black text-slate-700 mb-2">Lá»›p chÆ°a cÃ³ há»c viÃªn</h2>
      <p class="text-slate-500 max-w-md mx-auto">KhÃ´ng cÃ³ há»c viÃªn nÃ o Ä‘áº¡t Ä‘iá»u kiá»‡n ghi danh (Äang há»c/ÄÃ£ xÃ¡c nháº­n) Ä‘á»ƒ nháº­p Ä‘iá»ƒm.</p>
    </div>

    <template v-else>
      <section class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">SÄ© sá»‘ lá»›p</p>
            <strong class="text-2xl font-black text-slate-800">{{ rows.length }}</strong>
          </div>
          <div class="w-10 h-10 bg-slate-50 text-slate-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Tá»· lá»‡ Äáº¡t</p>
            <strong class="text-2xl font-black text-emerald-600">{{ stats.passRate }}%</strong>
          </div>
          <div class="w-10 h-10 bg-emerald-50 text-emerald-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Äiá»ƒm TB Lá»›p</p>
            <strong class="text-2xl font-black text-blue-600">{{ stats.avgScore }}</strong>
          </div>
          <div class="w-10 h-10 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">ChuyÃªn cáº§n TB</p>
            <strong class="text-2xl font-black text-indigo-600">{{ stats.avgAttendance }}%</strong>
          </div>
          <div class="w-10 h-10 bg-indigo-50 text-indigo-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg></div>
        </div>
      </section>

      <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col relative">
        <div class="p-4 border-b border-slate-100 bg-slate-50/80 flex flex-col lg:flex-row lg:items-center justify-between gap-4">
          <div>
            <h2 class="text-base font-bold text-slate-800">Danh sÃ¡ch há»c viÃªn</h2>
            <p class="text-[11px] font-medium text-slate-500 mt-0.5">TB = (Giá»¯a ká»³ Ã— 0.4) + (Cuá»‘i ká»³ Ã— 0.6). Äiá»u kiá»‡n Äáº¡t: TB â‰¥ 5.0 & ChuyÃªn cáº§n â‰¥ 70%.</p>
          </div>
          
          <div class="flex items-center gap-3">
            <div class="relative">
              <svg class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
              <input 
                v-model="searchQuery" 
                type="text" 
                placeholder="TÃ¬m tÃªn há»c viÃªn..." 
                class="pl-9 pr-4 py-1.5 text-sm bg-white border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-100 focus:border-blue-400 outline-none transition-all w-full sm:w-64"
              />
            </div>
            <a-tooltip title="Äáº·t nhanh 100% chuyÃªn cáº§n cho há»c viÃªn chÆ°a nháº­p">
              <button @click="quickFillAttendance" class="p-2 bg-white border border-slate-200 rounded-lg text-slate-500 hover:text-emerald-600 hover:bg-emerald-50 transition-colors">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" /></svg>
              </button>
            </a-tooltip>
          </div>
        </div>

        <div class="p-0 overflow-x-auto custom-scrollbar">
          <a-table
            :data-source="filteredRows"
            :columns="columns"
            row-key="studentId"
            size="middle"
            class="enterprise-table"
            :pagination="false"
            :scroll="{ x: 1050 }"
          >
            <template #bodyCell="{ column, record, index }">
              
              <template v-if="column.key === 'index'">
                <span class="text-slate-400 font-semibold">{{ index + 1 }}</span>
              </template>

              <template v-else-if="column.key === 'student'">
                <div class="flex items-center gap-3 py-1">
                  <div class="w-9 h-9 rounded-xl flex items-center justify-center text-[10px] font-black text-white shrink-0 shadow-sm border border-white/20" :style="{ background: avatarBg(record.studentNameSnapshot) }">
                    {{ getInitials(record.studentNameSnapshot) }}
                  </div>
                  <div class="min-w-0 flex flex-col">
                    <span class="font-bold text-slate-800 text-[13px] truncate" :title="record.studentNameSnapshot">{{ record.studentNameSnapshot }}</span>
                    <span class="text-[10px] text-slate-500 font-mono mt-0.5">{{ record.studentId.substring(0,8).toUpperCase() }}</span>
                  </div>
                </div>
              </template>

              <template v-else-if="column.key === 'attendancePercent'">
                <div class="flex items-center gap-2">
                  <a-input-number 
                    v-model:value="record.attendancePercent" 
                    :min="0" :max="100" :step="1" 
                    class="w-[70px] custom-number-input" 
                    @change="markDirty"
                  />
                  <span class="text-xs font-bold text-slate-400">%</span>
                </div>
              </template>

              <template v-else-if="column.key === 'midtermScore'">
                <a-input-number 
                  v-model:value="record.midtermScore" 
                  :min="0" :max="10" :step="0.25" 
                  class="w-[85px] custom-number-input" 
                  placeholder="0.0"
                  @change="markDirty"
                />
              </template>

              <template v-else-if="column.key === 'finalScore'">
                <a-input-number 
                  v-model:value="record.finalScore" 
                  :min="0" :max="10" :step="0.25" 
                  class="w-[85px] custom-number-input" 
                  placeholder="0.0"
                  @change="markDirty"
                />
              </template>

              <template v-else-if="column.key === 'average'">
                <div :class="['inline-flex items-center justify-center px-3 py-1.5 rounded-lg border shadow-sm min-w-[50px]', getAvgBadgeClass(average(record))]">
                  <span class="text-[14px] font-black leading-none">
                    {{ average(record).toFixed(1) }}
                  </span>
                </div>
              </template>

              <template v-else-if="column.key === 'result'">
                <span class="whitespace-nowrap inline-flex">
                  <span :class="['px-2.5 py-1 text-[11px] font-black rounded-md border uppercase tracking-wider', passed(record) ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-rose-50 text-rose-700 border-rose-200']">
                    {{ passed(record) ? 'Äáº¡t' : 'KhÃ´ng Äáº¡t' }}
                  </span>
                </span>
              </template>

              <template v-else-if="column.key === 'aiGrading'">
                <a-button 
                  type="primary" 
                  size="small" 
                  class="bg-blue-600 hover:bg-blue-700 text-white rounded-md text-[10px] font-bold px-2 py-1 h-7 border-none flex items-center justify-center cursor-pointer"
                  @click="openGradingModal(record)"
                >
                  AI Cháº¥m
                </a-button>
              </template>

              <template v-else-if="column.key === 'feedback'">
                <a-textarea 
                  v-model:value="record.feedback" 
                  placeholder="Nháº­p nháº­n xÃ©t..." 
                  :auto-size="{ minRows: 1, maxRows: 3 }"
                  class="text-xs bg-slate-50 border-slate-200 focus:bg-white rounded-lg custom-scrollbar min-w-[180px]"
                  @change="markDirty"
                />
              </template>

            </template>
            <template #emptyText>
              <div class="py-10 text-slate-400">KhÃ´ng tÃ¬m tháº¥y há»c viÃªn phÃ¹ há»£p.</div>
            </template>
          </a-table>
        </div>
      </section>
    </template>

    <!-- Modal Cháº¥m Äiá»ƒm & Nháº­n XÃ©t AI -->
    <a-modal
      v-model:visible="gradingModalVisible"
      title="ðŸ¤– Trá»£ lÃ½ Cháº¥m Ä‘iá»ƒm & Nháº­n xÃ©t Tá»± Ä‘á»™ng (AI Grading)"
      :footer="null"
      width="700px"
      centered
      @cancel="closeGradingModal"
    >
      <div class="space-y-4 py-2">
        <div class="grid grid-cols-2 gap-3 bg-slate-50 p-3 rounded-xl border border-slate-100 text-xs">
          <div>
            <span class="text-slate-400 block mb-0.5">Há»c viÃªn:</span>
            <strong class="text-slate-700 text-[13px]">{{ activeGradingRow?.studentNameSnapshot }}</strong>
          </div>
          <div>
            <span class="text-slate-400 block mb-0.5">KhÃ³a há»c / Lá»›p:</span>
            <strong class="text-slate-700 text-[13px]">{{ activeGradingRow?.classNameSnapshot }}</strong>
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-xs font-bold text-slate-600 uppercase tracking-wider block">1. Nháº­p BÃ i lÃ m / BÃ i luáº­n cá»§a Há»c viÃªn:</label>
          <a-textarea
            v-model:value="studentSubmission"
            placeholder="DÃ¡n cÃ¢u tráº£ lá»i ngáº¯n, bÃ i viáº¿t hoáº·c bÃ¡o cÃ¡o dá»± Ã¡n cá»§a há»c viÃªn táº¡i Ä‘Ã¢y..."
            :rows="6"
            class="text-xs rounded-xl"
          />
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-600 uppercase tracking-wider block">2. Loáº¡i bÃ i kiá»ƒm tra:</label>
            <a-select v-model:value="testType" class="w-full text-xs rounded-lg">
              <a-select-option value="midterm">Äiá»ƒm Giá»¯a Ká»³ (Há»c trÃ¬nh 40%)</a-select-option>
              <a-select-option value="final">Äiá»ƒm Cuá»‘i Ká»³ (BÃ¡o cÃ¡o dá»± Ã¡n 60%)</a-select-option>
            </a-select>
          </div>
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-600 uppercase tracking-wider block">3. TiÃªu chÃ­ cháº¥m (Barem):</label>
            <a-select v-model:value="gradingRubric" class="w-full text-xs rounded-lg">
              <a-select-option value="programming">Äáº§y Ä‘á»§ chá»©c nÄƒng, logic code, tá»‘i Æ°u</a-select-option>
              <a-select-option value="essay">TÃ­nh há»c thuáº­t, láº­p luáº­n, vÄƒn phong</a-select-option>
              <a-select-option value="short_answer">Äá»™ chÃ­nh xÃ¡c thÃ´ng tin, ngáº¯n gá»n</a-select-option>
            </a-select>
          </div>
        </div>

        <div class="flex justify-end gap-2 pt-2 border-t border-slate-100">
          <a-button @click="closeGradingModal" class="rounded-lg h-9">Há»§y</a-button>
          <a-button
            type="primary"
            class="bg-blue-600 hover:bg-blue-700 text-white rounded-lg h-9 font-bold flex items-center gap-1.5 cursor-pointer"
            :loading="gradingAiLoading"
            :disabled="!studentSubmission.trim()"
            @click="runAiGrading"
          >
            ðŸ¤– Cháº¥m Ä‘iá»ƒm báº±ng AI
          </a-button>
        </div>

        <!-- Káº¿t quáº£ AI Cháº¥m -->
        <div v-if="aiGradingResult" class="mt-4 p-4 border border-blue-100 bg-blue-50/20 rounded-2xl space-y-3 animate-fade-in">
          <div class="flex items-center justify-between border-b border-blue-100/50 pb-2">
            <h4 class="text-xs font-black text-blue-700 uppercase tracking-wider flex items-center gap-1.5">
              <span>ðŸŽ¯ Káº¿t quáº£ Ä‘Ã¡nh giÃ¡ tá»« AI</span>
            </h4>
            <div class="flex items-center gap-2">
              <span class="text-xs text-slate-500">Äiá»ƒm Ä‘á» xuáº¥t:</span>
              <div class="px-2.5 py-1 bg-blue-600 text-white text-sm font-black rounded-lg shadow">
                {{ aiGradingResult.score }}/10
              </div>
            </div>
          </div>

          <div class="space-y-2 text-xs leading-relaxed text-slate-600">
            <div>
              <strong>Chi tiáº¿t Ä‘Ã¡nh giÃ¡:</strong>
              <div class="pl-3 mt-1 space-y-1">
                <p>â€¢ <strong>Æ¯u Ä‘iá»ƒm:</strong> {{ aiGradingResult.strengths }}</p>
                <p>â€¢ <strong>Háº¡n cháº¿:</strong> {{ aiGradingResult.weaknesses }}</p>
              </div>
            </div>
            <div class="pt-2 border-t border-dashed border-slate-200/60">
              <strong>Nháº­n xÃ©t Ä‘á» xuáº¥t (Feedback):</strong>
              <p class="mt-1 bg-white p-2.5 rounded-lg border border-slate-100 italic text-slate-700">
                {{ aiGradingResult.feedback }}
              </p>
            </div>
          </div>

          <div class="flex justify-end pt-2">
            <a-button
              type="primary"
              class="bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg h-9 font-bold flex items-center gap-1.5 cursor-pointer"
              @click="applyAiGrading"
            >
              <CheckOutlined />
              Ãp dá»¥ng Ä‘iá»ƒm & nháº­n xÃ©t
            </a-button>
          </div>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from 'vue'
import { message } from 'ant-design-vue'
import { CheckOutlined } from '@ant-design/icons-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { resultApi } from '@/api/resultApi'
import { aiApi } from '@/api/aiApi'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({ classId: { type: String, default: '' } })
const auth = useAuthStore()

const loading = ref(false)
const saving = ref(false)
const isDirty = ref(false) // Tráº¡ng thÃ¡i chÆ°a lÆ°u
const searchQuery = ref('')

const classes = ref([])
const selectedClassId = ref(props.classId || undefined)
const rows = ref([])

// Cáº¥u trÃºc báº£ng (Cá»‘ Ä‘á»‹nh cá»™t tÃªn vÃ  fix Ä‘á»™ rá»™ng)
const columns = [
  { title: '#', key: 'index', width: 50, align: 'center', fixed: 'left' },
  { title: 'ThÃ´ng tin Há»c viÃªn', key: 'student', width: 240, fixed: 'left' },
  { title: 'ChuyÃªn cáº§n', key: 'attendancePercent', width: 130 },
  { title: 'Giá»¯a ká»³ (40%)', key: 'midtermScore', width: 120 },
  { title: 'Cuá»‘i ká»³ (60%)', key: 'finalScore', width: 120 },
  { title: 'Trung bÃ¬nh', key: 'average', width: 100, align: 'center' },
  { title: 'Xáº¿p loáº¡i', key: 'result', width: 130, align: 'center' },
  { title: 'AI Cháº¥m ðŸ¤–', key: 'aiGrading', width: 100, align: 'center' },
  { title: 'Pháº£n há»“i tá»« Giáº£ng viÃªn', key: 'feedback', minWidth: 200 }
]

// ================= LOGIC & CALCULATION =================
function activeEnrollments(items) {
  return items.filter(x => ['Confirmed', 'Studying', 2, 3, '2', '3'].includes(x.status))
}

function average(row) {
  return Number(row.midtermScore || 0) * 0.4 + Number(row.finalScore || 0) * 0.6
}

function passed(row) {
  return average(row) >= 5 && Number(row.attendancePercent || 0) >= 70
}

function markDirty() {
  isDirty.value = true
}

const filteredRows = computed(() => {
  if (!searchQuery.value) return rows.value
  const q = searchQuery.value.toLowerCase()
  return rows.value.filter(r => r.studentNameSnapshot?.toLowerCase().includes(q))
})

function quickFillAttendance() {
  rows.value.forEach(r => {
    if (r.attendancePercent == null || r.attendancePercent === 0) {
      r.attendancePercent = 100
      isDirty.value = true
    }
  })
  message.success('ÄÃ£ Ä‘iá»n nhanh 100% chuyÃªn cáº§n cho cÃ¡c Ã´ trá»‘ng.')
}

// ================= THá»NG KÃŠ REAL-TIME =================
const stats = computed(() => {
  if (rows.value.length === 0) return { passRate: 0, avgScore: '0.0', avgAttendance: 0 }
  const totalStudents = rows.value.length
  const passedStudents = rows.value.filter(r => passed(r)).length
  const totalScore = rows.value.reduce((acc, curr) => acc + average(curr), 0)
  const totalAttendance = rows.value.reduce((acc, curr) => acc + Number(curr.attendancePercent || 0), 0)

  return {
    passRate: Math.round((passedStudents / totalStudents) * 100),
    avgScore: (totalScore / totalStudents).toFixed(1),
    avgAttendance: Math.round(totalAttendance / totalStudents)
  }
})

// ================= UI HELPERS =================
function getInitials(name) {
  if (!name) return 'HV'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
  'linear-gradient(135deg, #10b981 0%, #047857 100%)',
  'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)',
  'linear-gradient(135deg, #8b5cf6 0%, #5b21b6 100%)',
  'linear-gradient(135deg, #ec4899 0%, #be185d 100%)',
]

function avatarBg(name) {
  if (!name) return 'linear-gradient(135deg, #94a3b8 0%, #475569 100%)'
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

function getAvgBadgeClass(score) {
  if (score == null || score === 0) return 'bg-slate-50 border-slate-200 text-slate-400'
  if (score >= 8) return 'bg-emerald-50 border-emerald-200 text-emerald-700'
  if (score >= 5) return 'bg-blue-50 border-blue-200 text-blue-700'
  return 'bg-rose-50 border-rose-200 text-rose-700'
}

// ================= IMPORT / EXPORT CSV =================
const fileInput = ref(null)

function triggerImport() {
  fileInput.value.click()
}

function handleImportCSV(event) {
  const file = event.target.files[0]
  if (!file) return

  // Frontend Mock CSV Parser for demonstration
  const reader = new FileReader()
  reader.onload = (e) => {
    try {
      const text = e.target.result
      const lines = text.split('\n')
      let count = 0
      
      // Simple parser (assuming ID is first column, skip header)
      for (let i = 1; i < lines.length; i++) {
        const parts = lines[i].split(',')
        if (parts.length >= 4) {
          const sId = parts[0].trim()
          const rowMatch = rows.value.find(r => r.studentId === sId)
          if (rowMatch) {
            rowMatch.attendancePercent = parseFloat(parts[2]) || 0
            rowMatch.midtermScore = parseFloat(parts[3]) || 0
            rowMatch.finalScore = parseFloat(parts[4]) || 0
            if (parts[7]) rowMatch.feedback = parts[7].replace(/['"]+/g, '').trim()
            count++
            isDirty.value = true
          }
        }
      }
      message.success(`ÄÃ£ nháº­p Ä‘iá»ƒm thÃ nh cÃ´ng cho ${count} há»c viÃªn. Äá»«ng quÃªn báº¥m LÆ°u!`)
    } catch(err) {
      message.error('Äá»‹nh dáº¡ng file khÃ´ng há»£p lá»‡.')
    }
  }
  reader.readAsText(file)
  event.target.value = '' // reset
}

function downloadTemplate() {
  const header = "MÃ£ Há»c ViÃªn,TÃªn Há»c ViÃªn,ChuyÃªn Cáº§n (%),Äiá»ƒm Giá»¯a Ká»³,Äiá»ƒm Cuá»‘i Ká»³,Äiá»ƒm TB,Xáº¿p Loáº¡i,Nháº­n XÃ©t\n"
  let content = header
  
  if (rows.value.length > 0) {
    content += rows.value.map(r => `${r.studentId},"${r.studentNameSnapshot}",${r.attendancePercent||0},${r.midtermScore||0},${r.finalScore||0},0,"",""`).join('\n')
  } else {
    content += 'HV001,"Nguyá»…n VÄƒn A",100,8.5,9.0,0,"","Há»c tá»‘t"'
  }

  downloadBlob(content, 'Template_NhapDiem.csv')
}

function exportToCSV() {
  const header = "MÃ£ Há»c ViÃªn,TÃªn Há»c ViÃªn,ChuyÃªn Cáº§n (%),Äiá»ƒm Giá»¯a Ká»³,Äiá»ƒm Cuá»‘i Ká»³,Äiá»ƒm TB,Xáº¿p Loáº¡i,Nháº­n XÃ©t\n"
  const content = header + rows.value.map(r => {
    return `${r.studentId},"${r.studentNameSnapshot}",${r.attendancePercent||0},${r.midtermScore||0},${r.finalScore||0},${average(r).toFixed(1)},"${passed(r)?'Äáº¡t':'KhÃ´ng Ä‘áº¡t'}","${r.feedback||''}"`
  }).join('\n')
  
  downloadBlob(content, `BangDiem_${classes.value.find(c => c.id === selectedClassId.value)?.classCode || 'Lop'}.csv`)
}

function downloadBlob(content, filename) {
  // Use BOM for UTF-8 Excel support
  const blob = new Blob(["\uFEFF" + content], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement("a")
  link.setAttribute("href", url)
  link.setAttribute("download", filename)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

// ================= DATA FETCHING =================
async function loadBaseData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    if (!selectedClassId.value && classes.value.length) selectedClassId.value = classes.value[0].id
    await loadClassData()
  } catch(err) {
    message.error('Lá»—i khi táº£i danh sÃ¡ch lá»›p.')
  } finally {
    loading.value = false
  }
}

async function loadClassData() {
  if (!selectedClassId.value) return
  loading.value = true
  searchQuery.value = ''
  isDirty.value = false
  try {
    const selectedClass = classes.value.find(x => x.id === selectedClassId.value) || await classApi.getById(selectedClassId.value)
    const [enrollments, results] = await Promise.all([
      enrollmentApi.getByClass(selectedClassId.value),
      resultApi.getByClass(selectedClassId.value)
    ])
    
    const resultByStudent = new Map((results || []).map(x => [x.studentId, x]))
    
    rows.value = activeEnrollments(enrollments || []).map(en => {
      const existing = resultByStudent.get(en.studentId)
      return {
        id: existing?.id || null,
        studentId: en.studentId,
        studentNameSnapshot: en.studentNameSnapshot,
        courseId: en.courseId || selectedClass.courseId,
        courseNameSnapshot: en.courseNameSnapshot || selectedClass.courseNameSnapshot,
        classId: en.classId || selectedClass.id,
        classNameSnapshot: en.classNameSnapshot || selectedClass.className,
        midtermScore: existing?.midtermScore !== undefined ? Number(existing.midtermScore) : null,
        finalScore: existing?.finalScore !== undefined ? Number(existing.finalScore) : null,
        attendancePercent: existing?.attendancePercent !== undefined ? Number(existing.attendancePercent) : 100,
        feedback: existing?.feedback || ''
      }
    })
  } catch(err) {
    message.error('KhÃ´ng thá»ƒ táº£i danh sÃ¡ch há»c viÃªn.')
  } finally {
    loading.value = false
  }
}

async function saveAll() {
  saving.value = true
  try {
    for (const row of rows.value) {
      const payload = {
        studentId: row.studentId,
        courseId: row.courseId,
        courseNameSnapshot: row.courseNameSnapshot,
        classId: row.classId,
        classNameSnapshot: row.classNameSnapshot,
        midtermScore: Number(row.midtermScore || 0),
        finalScore: Number(row.finalScore || 0),
        attendancePercent: Number(row.attendancePercent || 0),
        resultStatus: passed(row) ? 2 : 3, // 2: Passed, 3: Failed
        averageScore: average(row),
        feedback: row.feedback,
        evaluatedByTeacherId: auth.user.referenceId,
        evaluatedByTeacherName: auth.user.fullName || auth.user.username
      }
      const saved = row.id ? await resultApi.update(row.id, payload) : await resultApi.create(payload)
      row.id = saved.id
    }
    isDirty.value = false // Táº¯t cá» cáº£nh bÃ¡o
    message.success('ToÃ n bá»™ báº£ng Ä‘iá»ƒm Ä‘Ã£ Ä‘Æ°á»£c lÆ°u thÃ nh cÃ´ng.')
    await loadClassData()
  } catch (error) {
    message.error(error.message || 'Há»‡ thá»‘ng gáº·p lá»—i, khÃ´ng lÆ°u Ä‘Æ°á»£c káº¿t quáº£.')
  } finally {
    saving.value = false
  }
}

// Cáº£nh bÃ¡o ngÆ°á»i dÃ¹ng khi reload page náº¿u cÃ³ thay Ä‘á»•i chÆ°a lÆ°u
window.addEventListener('beforeunload', (e) => {
  if (isDirty.value) {
    e.preventDefault()
    e.returnValue = ''
  }
})

// AI Grading State & Methods
const gradingModalVisible = ref(false)
const gradingAiLoading = ref(false)
const activeGradingRow = ref(null)
const studentSubmission = ref('')
const testType = ref('final')
const gradingRubric = ref('programming')
const aiGradingResult = ref(null)

function openGradingModal(row) {
  activeGradingRow.value = row
  studentSubmission.value = ''
  aiGradingResult.value = null
  gradingModalVisible.value = true
}

function closeGradingModal() {
  gradingModalVisible.value = false
  activeGradingRow.value = null
}

async function runAiGrading() {
  if (!studentSubmission.value.trim()) return
  gradingAiLoading.value = true
  aiGradingResult.value = null

  const row = activeGradingRow.value
  const rubricText = {
    programming: 'Táº­p trung cháº¥m Ä‘iá»ƒm theo: logic giáº£i thuáº­t, cáº¥u trÃºc mÃ£ nguá»“n tá»‘i Æ°u, tÃ­nh hoÃ n thiá»‡n cá»§a cÃ¡c chá»©c nÄƒng vÃ  xá»­ lÃ½ lá»—i.',
    essay: 'Táº­p trung cháº¥m Ä‘iá»ƒm theo: tÃ­nh há»c thuáº­t, láº­p luáº­n cháº·t cháº½, vÄƒn phong máº¡ch láº¡c, Ä‘Ãºng trá»ng tÃ¢m vÃ  cáº¥u trÃºc luáº­n vÄƒn.',
    short_answer: 'Táº­p trung cháº¥m Ä‘iá»ƒm theo: Ä‘á»™ chÃ­nh xÃ¡c cá»§a cÃ¢u tráº£ lá»i, sá»± cÃ´ Ä‘á»ng, Ä‘Ãºng tá»« khÃ³a cá»‘t lÃµi.'
  }[gradingRubric.value]

  const promptContext = `
HÃ£y lÃ  má»™t giÃ¡o viÃªn chuyÃªn mÃ´n cháº¥m Ä‘iá»ƒm bÃ i kiá»ƒm tra mÃ´n há»c "${row.courseNameSnapshot || 'CÃ´ng nghá»‡ thÃ´ng tin'}".
ThÃ´ng tin há»c viÃªn:
- TÃªn: ${row.studentNameSnapshot}
- Loáº¡i bÃ i thi: ${testType.value === 'midterm' ? 'Giá»¯a ká»³ (trá»ng sá»‘ 40%)' : 'Cuá»‘i ká»³ (trá»ng sá»‘ 60%)'}

Ná»™i dung bÃ i lÃ m cá»§a há»c viÃªn:
"${studentSubmission.value}"

TiÃªu chÃ­ cháº¥m (Rubric):
${rubricText}

Nhiá»‡m vá»¥ cá»§a báº¡n lÃ :
1. ÄÆ°a ra Ä‘iá»ƒm sá»‘ khÃ¡ch quan trÃªn thang Ä‘iá»ƒm 10 (chá»‰ cháº¥p nháº­n sá»‘ nguyÃªn hoáº·c sá»‘ tháº­p phÃ¢n chia háº¿t cho 0.25 hoáº·c 0.5, vÃ­ dá»¥: 7.0, 7.25, 8.5, 9.0).
2. Viáº¿t phÃ¢n tÃ­ch tÃ³m táº¯t Ä‘iá»ƒm máº¡nh (strengths) vÃ  Ä‘iá»ƒm yáº¿u cáº§n cáº£i thiá»‡n (weaknesses) cá»§a bÃ i lÃ m (ngáº¯n gá»n, tá»‘i Ä‘a 2 cÃ¢u má»—i pháº§n).
3. Äá» xuáº¥t nháº­n xÃ©t (feedback) ngáº¯n gá»n (khoáº£ng 2-3 cÃ¢u) báº±ng tiáº¿ng Viá»‡t lá»‹ch sá»±, mang tÃ­nh khuyáº¿n khÃ­ch Ä‘á»™ng viÃªn vÃ  thiáº¿t thá»±c Ä‘á»ƒ giÃ¡o viÃªn gá»­i trá»±c tiáº¿p cho há»c viÃªn.

Äá»‹nh dáº¡ng Ä‘áº§u ra Báº®T BUá»˜C lÃ  chuá»—i JSON há»£p lá»‡ cÃ³ dáº¡ng:
{
  "score": 8.5,
  "strengths": "mÃ´ táº£ Æ°u Ä‘iá»ƒm...",
  "weaknesses": "mÃ´ táº£ háº¡n cháº¿...",
  "feedback": "nháº­n xÃ©t gá»£i Ã½..."
}
  `

  try {
    const resData = await aiApi.complete({
      prompt: promptContext,
      jsonMode: true,
      maxOutputTokens: 1200
    })
    const rawText = resData.text || ''

    
    // Parse json
    const cleanJson = JSON.parse(rawText.trim())
    aiGradingResult.value = {
      score: cleanJson.score != null ? parseFloat(cleanJson.score) : 7.0,
      strengths: cleanJson.strengths || 'LÃ m bÃ i tÆ°Æ¡ng Ä‘á»‘i Ä‘áº§y Ä‘á»§.',
      weaknesses: cleanJson.weaknesses || 'Cáº§n tá»‘i Æ°u thÃªm cáº¥u trÃºc.',
      feedback: cleanJson.feedback || 'BÃ i lÃ m khÃ¡, cáº§n tiáº¿p tá»¥c phÃ¡t huy.'
    }
  } catch (err) {
    console.error(err)
    message.error('Gáº·p lá»—i khi cháº¥m Ä‘iá»ƒm tá»± Ä‘á»™ng báº±ng AI.')
    // Fallback simulated result
    aiGradingResult.value = {
      score: 7.5,
      strengths: 'CÃ³ hiá»ƒu kiáº¿n thá»©c bÃ i há»c, cáº¥u trÃºc bÃ i luáº­n tÆ°Æ¡ng Ä‘á»‘i rÃµ rÃ ng.',
      weaknesses: 'ChÆ°a Ä‘i sÃ¢u phÃ¢n tÃ­ch ká»¹ thuáº­t, má»™t sá»‘ pháº§n cÃ²n chung chung.',
      feedback: 'Em Ä‘Ã£ náº¯m Ä‘Æ°á»£c pháº§n lá»›n kiáº¿n thá»©c cá»‘t lÃµi. HÃ£y cá»‘ gáº¯ng phÃ¡t huy thÃªm kháº£ nÄƒng tá»‘i Æ°u hÃ³a trong cÃ¡c bÃ i táº­p káº¿ tiáº¿p nhÃ©!'
    }
  } finally {
    gradingAiLoading.value = false
  }
}

function applyAiGrading() {
  if (!aiGradingResult.value || !activeGradingRow.value) return
  
  const targetRow = rows.value.find(r => r.studentId === activeGradingRow.value.studentId)
  if (targetRow) {
    const valScore = aiGradingResult.value.score
    if (testType.value === 'midterm') {
      targetRow.midtermScore = valScore
    } else {
      targetRow.finalScore = valScore
    }
    targetRow.feedback = aiGradingResult.value.feedback
    isDirty.value = true
    message.success(`ÄÃ£ Ã¡p dá»¥ng káº¿t quáº£ AI cho há»c viÃªn ${targetRow.studentNameSnapshot}!`)
  }
  
  closeGradingModal()
}

watch(() => props.classId, (value) => { if (value) selectedClassId.value = value })
onMounted(loadBaseData)
</script>

<style scoped>
/* Hiá»‡u á»©ng chá»›p nhÃ¡y cáº£nh bÃ¡o (MÃ u cam) */
.animate-pulse-fast {
  animation: pulse-border 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
@keyframes pulse-border {
  0%, 100% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.4); }
  50% { box-shadow: 0 0 0 4px rgba(245, 158, 11, 0); }
}

.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-5px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Custom Input Number cho báº£ng Ä‘iá»ƒm */
:deep(.custom-number-input) {
  border-radius: 8px;
  border-color: #e2e8f0;
}
:deep(.custom-number-input:hover),
:deep(.custom-number-input:focus-within) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
}
:deep(.custom-number-input .ant-input-number-input) {
  font-weight: 700;
  color: #1e293b;
  text-align: center;
}

/* Custom Select cho Toolbar */
:deep(.custom-select .ant-select-selector) {
  border-radius: 12px !important;
  padding: 8px 16px !important;
  height: auto !important;
  border-color: #e2e8f0 !important;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
}
:deep(.custom-select.ant-select-focused .ant-select-selector) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1) !important;
}

/* Custom Table Enterprise */
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 10px 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}

/* Scrollbar */
.custom-scrollbar::-webkit-scrollbar { width: 6px; height: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
</style>
