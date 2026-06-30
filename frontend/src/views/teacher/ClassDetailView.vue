<template>
  <div class="space-y-6">
    <PageHeader>
      <template #title>
        <div class="flex items-center gap-3">
          <h1 class="text-2xl font-black text-slate-800">{{ classInfo?.className || 'Äang táº£i thÃ´ng tin lá»›p...' }}</h1>
          <span v-if="classInfo" class="px-2.5 py-0.5 text-[10px] font-black uppercase tracking-wider bg-blue-100 text-blue-700 rounded-lg border border-blue-200 shadow-sm">
            {{ classInfo?.classCode || 'â€”' }}
          </span>
        </div>
      </template>
      <template #subtitle>
        <p class="text-slate-500 font-medium mt-1">{{ classInfo?.courseNameSnapshot || 'Quáº£n lÃ½ toÃ n diá»‡n dá»¯ liá»‡u lá»›p há»c, lá»‹ch trÃ¬nh vÃ  káº¿t quáº£.' }}</p>
      </template>
      <template #actions>
        <router-link to="/teacher/classes">
          <button class="px-4 py-2.5 bg-white border border-slate-200 text-slate-700 font-bold rounded-xl shadow-sm hover:bg-slate-50 transition-colors active:scale-95 text-sm">
            Quay láº¡i
          </button>
        </router-link>

        <button v-if="classInfo" @click="goToResults" class="px-5 py-2.5 bg-purple-50 text-purple-700 border border-purple-200 font-bold rounded-xl shadow-sm hover:bg-purple-100 transition-colors flex items-center gap-2 active:scale-95 text-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
          Nháº­p káº¿t quáº£
        </button>

        <button v-if="classInfo" @click="goToAttendance" class="px-5 py-2.5 bg-blue-600 text-white font-bold rounded-xl shadow-sm hover:bg-blue-700 transition-colors flex items-center gap-2 active:scale-95 text-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>
          Má»Ÿ Ä‘iá»ƒm danh
        </button>
      </template>
    </PageHeader>

    <section class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex flex-col justify-center transition-all hover:shadow-md hover:border-blue-200">
        <div class="flex items-center gap-4 mb-3">
          <div class="p-3 bg-blue-50 text-blue-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg></div>
          <div>
            <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">SÄ© sá»‘ lá»›p</span>
            <strong class="text-lg font-black text-slate-800">{{ classInfo?.currentStudents || 0 }} / {{ classInfo?.maxStudents || 0 }}</strong>
          </div>
        </div>
        <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden shadow-inner">
          <div class="h-full bg-blue-500 rounded-full transition-all" :style="{ width: `${Math.min(((classInfo?.currentStudents || 0) / (classInfo?.maxStudents || 1)) * 100, 100)}%` }"></div>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-emerald-200">
        <div class="p-3 bg-emerald-50 text-emerald-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m3-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Äá»‹a Ä‘iá»ƒm</span>
          <strong class="text-lg font-black text-slate-800">{{ classInfo?.room ? `PhÃ²ng ${classInfo.room}` : 'ChÆ°a xáº¿p phÃ²ng' }}</strong>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-indigo-200">
        <div class="p-3 bg-indigo-50 text-indigo-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Khai giáº£ng</span>
          <strong class="text-lg font-black text-slate-800">{{ formatDate(classInfo?.startDate) }}</strong>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-amber-200">
        <div class="p-3 bg-amber-50 text-amber-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Dá»± kiáº¿n báº¿ giáº£ng</span>
          <strong class="text-lg font-black text-slate-800">{{ formatDate(classInfo?.endDate) }}</strong>
        </div>
      </div>
    </section>

    <section class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden min-h-[500px]">
      <div class="border-b border-slate-100 px-6 pt-4 bg-slate-50/50">
        <a-tabs v-model:activeKey="activeTab" class="custom-tabs">
          <a-tab-pane key="students" :tab="`Há»c viÃªn (${enrollments.length})`" />
          <a-tab-pane key="schedule" :tab="`Lá»‹ch há»c (${schedules.length})`" />
          <a-tab-pane key="attendance" :tab="`Buá»•i Ä‘iá»ƒm danh (${sessions.length})`" />
          <a-tab-pane key="results" :tab="`Káº¿t quáº£ (${results.length})`" />
          <a-tab-pane key="ai-risk" tab="PhÃ¢n tÃ­ch rá»§i ro & Red Flags ðŸš©" />
        </a-tabs>
      </div>

      <div class="p-0">
        <div v-if="loading" class="py-24 flex justify-center">
          <LoadingSpinner size="lg" />
        </div>
        
        <a-table v-else-if="activeTab === 'students'" :data-source="enrollments" :columns="studentColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'student'">
              <div class="flex items-center gap-3">
                <div class="w-9 h-9 rounded-xl flex items-center justify-center text-[11px] font-black text-white shrink-0 shadow-sm border border-white/20" :style="{ background: avatarBg(record.studentNameSnapshot) }">
                  {{ getInitials(record.studentNameSnapshot) }}
                </div>
                <div>
                  <p class="font-bold text-slate-800 text-[13px]">{{ record.studentNameSnapshot || 'â€”' }}</p>
                  <p class="text-[10px] text-slate-500 font-mono mt-0.5 bg-slate-100 px-1.5 py-0.5 rounded w-max border border-slate-200">{{ record.studentId }}</p>
                </div>
              </div>
            </template>
            <template v-else-if="column.key === 'status'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border', enrollmentBadgeClass(record.status)]">
                {{ enrollmentLabel(record.status) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">ChÆ°a cÃ³ há»c viÃªn ghi danh vÃ o lá»›p nÃ y.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'schedule'" :data-source="schedules" :columns="scheduleColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'sessionNumber'">
              <span class="font-black text-slate-800">Buá»•i {{ record.sessionNumber }}</span>
            </template>
            
            <template v-if="column.key === 'time'">
              <div class="flex items-center gap-2 whitespace-nowrap">
                <div class="w-[90px] text-center px-2 py-1 bg-slate-100 border border-slate-200 text-slate-700 font-bold rounded-md text-[11px]">
                  {{ dayLabel(record.dayOfWeek) }}
                </div>
                <div class="w-[80px] text-center px-2 py-1 bg-blue-50 border border-blue-100 text-blue-700 font-bold rounded-md text-[11px]">
                  {{ shiftLabel(record.studyShift) }}
                </div>
                <div class="flex items-center gap-1.5 px-2 py-1 text-slate-600 font-mono text-[11px]">
                  <svg class="w-3.5 h-3.5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                  {{ formatTime(record.startTime) }} - {{ formatTime(record.endTime) }}
                </div>
              </div>
            </template>

            <template v-if="column.key === 'room'">
              <span class="font-bold text-slate-600">P.{{ record.room || 'â€”' }}</span>
            </template>
            <template v-if="column.key === 'topic'">
              <span class="text-slate-600 text-[13px]">{{ record.topic || 'â€”' }}</span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">ChÆ°a cÃ³ lá»‹ch trÃ¬nh Ä‘Æ°á»£c thiáº¿t láº­p.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'attendance'" :data-source="sessions" :columns="sessionColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'sessionNumber'">
              <span class="font-black text-slate-800">Buá»•i {{ record.sessionNumber }}</span>
            </template>
            <template v-if="column.key === 'topic'">
              <span class="text-slate-600 text-[13px]">{{ record.topic || 'â€”' }}</span>
            </template>
            <template v-if="column.key === 'status'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border flex items-center gap-1.5 w-max', sessionStatusClass(record.status)]">
                <span class="w-1.5 h-1.5 rounded-full" :class="String(record.status) === 'Locked' || Number(record.status) === 2 ? 'bg-rose-500' : 'bg-emerald-500'"></span>
                {{ sessionLabel(record.status) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">ChÆ°a cÃ³ phiÃªn Ä‘iá»ƒm danh nÃ o Ä‘Æ°á»£c táº¡o.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'results'" :data-source="results" :columns="resultColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 800 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'studentNameSnapshot'">
               <span class="font-bold text-slate-800 text-[13px]">{{ record.studentNameSnapshot || 'â€”' }}</span>
            </template>
            <template v-if="column.key === 'midtermScore' || column.key === 'finalScore'">
               <span :class="['font-black text-[13px]', getScoreColor(record[column.key])]">
                 {{ record[column.key] != null ? Number(record[column.key]).toFixed(1) : 'â€”' }}
               </span>
            </template>
            <template v-else-if="column.key === 'averageScore'">
              <div class="inline-flex items-center justify-center px-2 py-1 rounded bg-slate-50 border border-slate-100 min-w-[40px]">
                <strong :class="['text-sm', getScoreColor(record.averageScore)]">{{ record.averageScore != null ? Number(record.averageScore).toFixed(1) : 'â€”' }}</strong>
              </div>
            </template>
            <template v-else-if="column.key === 'resultStatus'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border uppercase tracking-wider', resultBadgeClass(record.resultStatus)]">
                {{ resultLabel(record.resultStatus) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">ChÆ°a cÃ³ dá»¯ liá»‡u káº¿t quáº£ há»c táº­p.</div></template>
        </a-table>

        <!-- Tab: PhÃ¢n tÃ­ch Rá»§i ro AI -->
        <div v-else-if="activeTab === 'ai-risk'" class="p-6 space-y-6">
          <!-- Overview KPI Cards -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="p-4 rounded-xl border border-rose-100 bg-rose-50/20 flex items-center justify-between">
              <div>
                <span class="text-[10px] font-bold text-rose-500 uppercase tracking-wider block">Nguy cÆ¡ cao (Red Flags)</span>
                <strong class="text-2xl font-black text-rose-600 block mt-1">{{ riskStats.highRiskCount }} há»c viÃªn</strong>
                <span class="text-[10px] text-slate-400 block mt-0.5">Äiá»ƒm TB &lt; 5.0 hoáº·c ChuyÃªn cáº§n &lt; 80%</span>
              </div>
              <div class="w-10 h-10 rounded-full bg-rose-505 text-white flex items-center justify-center font-bold" style="background: #f43f5e;">ðŸš©</div>
            </div>

            <div class="p-4 rounded-xl border border-amber-100 bg-amber-50/20 flex items-center justify-between">
              <div>
                <span class="text-[10px] font-bold text-amber-500 uppercase tracking-wider block">Cáº§n theo dÃµi (Yellow Flags)</span>
                <strong class="text-2xl font-black text-amber-600 block mt-1">{{ riskStats.mediumRiskCount }} há»c viÃªn</strong>
                <span class="text-[10px] text-slate-400 block mt-0.5">Äiá»ƒm TB 5.0 - 6.5 hoáº·c ChuyÃªn cáº§n 80% - 90%</span>
              </div>
              <div class="w-10 h-10 rounded-full bg-amber-500 text-white flex items-center justify-center font-bold" style="background: #f59e0b;">âš ï¸</div>
            </div>

            <div class="p-4 rounded-xl border border-emerald-100 bg-emerald-50/20 flex items-center justify-between">
              <div>
                <span class="text-[10px] font-bold text-emerald-500 uppercase tracking-wider block">Khu vá»±c an toÃ n (Green Zone)</span>
                <strong class="text-2xl font-black text-emerald-600 block mt-1">{{ riskStats.safeCount }} há»c viÃªn</strong>
                <span class="text-[10px] text-slate-400 block mt-0.5">Äiá»ƒm TB &ge; 6.5 vÃ  ChuyÃªn cáº§n &ge; 90%</span>
              </div>
              <div class="w-10 h-10 rounded-full bg-emerald-500 text-white flex items-center justify-center font-bold" style="background: #10b981;">âœ…</div>
            </div>
          </div>

          <!-- Red Flags Detail List -->
          <div class="border border-slate-200 rounded-2xl overflow-hidden bg-card-base">
            <div class="p-4 border-b border-slate-100 bg-slate-50/50 flex items-center justify-between">
              <div>
                <h3 class="text-xs font-bold text-slate-800 uppercase tracking-wider">Danh sÃ¡ch há»c viÃªn rá»§i ro há»c táº­p (Red Flags)</h3>
                <p class="text-[10px] text-slate-500 mt-0.5">Há»c viÃªn há»•ng kiáº¿n thá»©c hoáº·c cÃ³ nguy cÆ¡ khÃ´ng Ä‘áº¡t mÃ´n há»c.</p>
              </div>
            </div>

            <div class="overflow-x-auto">
              <table class="w-full text-left border-collapse text-xs">
                <thead>
                  <tr class="bg-slate-50 border-b border-slate-200 text-slate-500">
                    <th class="p-3 font-bold">ThÃ´ng tin há»c viÃªn</th>
                    <th class="p-3 font-bold text-center">ChuyÃªn cáº§n</th>
                    <th class="p-3 font-bold text-center">Äiá»ƒm thi TB</th>
                    <th class="p-3 font-bold">Lá»— há»•ng kiáº¿n thá»©c chÃ­nh</th>
                    <th class="p-3 font-bold text-right">HÃ nh Ä‘á»™ng</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="student in redFlagStudents" :key="student.studentId" class="border-b border-slate-100 last:border-b-0 hover:bg-slate-50/50">
                    <td class="p-3">
                      <div class="font-bold text-slate-800">{{ student.studentName }}</div>
                      <div class="text-[10px] text-slate-500 font-mono">{{ student.studentId }}</div>
                    </td>
                    <td class="p-3 text-center font-bold" :class="student.attendancePercent < 80 ? 'text-rose-500' : 'text-slate-700'">
                      {{ student.attendancePercent }}%
                    </td>
                    <td class="p-3 text-center font-bold" :class="student.avgScore < 5 ? 'text-rose-500' : 'text-slate-700'">
                      {{ student.avgScore.toFixed(1) }}
                    </td>
                    <td class="p-3 text-slate-600">
                      <span v-for="(gap, i) in student.knowledgeGaps" :key="i" class="inline-block bg-slate-100 text-[10px] px-2 py-0.5 rounded-md mr-1 mb-1 border border-slate-200">
                        {{ gap }}
                      </span>
                      <span v-if="!student.knowledgeGaps.length" class="text-slate-400 italic">ChÆ°a xÃ¡c Ä‘á»‹nh</span>
                    </td>
                    <td class="p-3 text-right">
                      <a-button 
                        type="primary" 
                        size="small" 
                        class="bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-[10px] font-bold px-2 py-1 h-7 border-none cursor-pointer flex items-center gap-1 inline-flex"
                        @click="openRiskPlan(student)"
                      >
                        ðŸ¤– LÃªn káº¿ hoáº¡ch
                      </a-button>
                    </td>
                  </tr>
                  <tr v-if="!redFlagStudents.length">
                    <td colspan="5" class="p-8 text-center text-slate-400 italic">
                      KhÃ´ng cÃ³ há»c viÃªn nÃ o gáº·p rá»§i ro há»c táº­p lá»›n (Red Flag) trong lá»›p nÃ y! ðŸŽ‰
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Drawer Káº¿ hoáº¡ch Phá»¥c há»“i AI -->
    <a-drawer
      v-model:visible="riskDrawerVisible"
      title="ðŸ¤– Káº¿ hoáº¡ch Phá»¥c há»“i Há»c táº­p AI"
      placement="right"
      width="550px"
      :footer="null"
      @close="closeRiskPlan"
    >
      <div class="space-y-5 text-xs text-slate-700">
        <div class="bg-slate-50 p-4 rounded-xl border border-slate-100 space-y-2">
          <h4 class="font-bold text-slate-800">ThÃ´ng tin phÃ¢n tÃ­ch:</h4>
          <div class="grid grid-cols-2 gap-2 text-[11px]">
            <p>â€¢ Há»c viÃªn: <strong>{{ activeRiskStudent?.studentName }}</strong></p>
            <p>â€¢ ChuyÃªn cáº§n: <strong class="text-rose-500">{{ activeRiskStudent?.attendancePercent }}%</strong></p>
            <p>â€¢ Äiá»ƒm TB hiá»‡n táº¡i: <strong class="text-rose-500">{{ activeRiskStudent?.avgScore?.toFixed(1) }}</strong></p>
            <p>â€¢ MÃ£ lá»›p: <strong>{{ classInfo?.classCode }}</strong></p>
          </div>
          <div class="pt-1.5 border-t border-slate-200">
            <span class="text-slate-400 block mb-1 text-[10px]">Chá»§ Ä‘á» há»•ng kiáº¿n thá»©c:</span>
            <span v-for="gap in activeRiskStudent?.knowledgeGaps" :key="gap" class="inline-block bg-rose-50 text-rose-600 px-2 py-0.5 rounded mr-1 text-[10px] border border-rose-100">
              {{ gap }}
            </span>
          </div>
        </div>

        <div class="flex justify-end">
          <a-button
            type="primary"
            class="bg-blue-600 hover:bg-blue-700 text-white rounded-lg h-9 font-bold flex items-center gap-1.5 cursor-pointer"
            :loading="riskAiLoading"
            @click="generateRiskPlan"
          >
            ðŸ¤– Táº¡o Káº¿ hoáº¡ch & Email báº±ng AI
          </a-button>
        </div>

        <div v-if="aiRiskPlanResult" class="space-y-4 animate-fade-in">
          <!-- Recovery Plan Section -->
          <div class="space-y-2">
            <h4 class="font-bold text-slate-800 text-[13px] border-l-2 border-blue-500 pl-2">
              ðŸ“š Lá»™ trÃ¬nh Ã´n táº­p bá»• trá»£ Ä‘á» xuáº¥t:
            </h4>
            <div class="bg-blue-50/20 p-3.5 border border-blue-100 rounded-xl leading-relaxed whitespace-pre-wrap">
              {{ aiRiskPlanResult.plan }}
            </div>
          </div>

          <!-- Email Draft Section -->
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <h4 class="font-bold text-slate-800 text-[13px] border-l-2 border-emerald-500 pl-2">
                âœ‰ï¸ Báº£n tháº£o email gá»­i há»c viÃªn/phá»¥ huynh:
              </h4>
              <a-button size="small" class="text-[10px] rounded-md h-6 cursor-pointer" @click="copyEmailDraft">Sao chÃ©p</a-button>
            </div>
            <div class="bg-emerald-50/20 p-3.5 border border-emerald-100 rounded-xl leading-relaxed whitespace-pre-wrap font-mono text-[11px]">
              {{ aiRiskPlanResult.email }}
            </div>
          </div>
        </div>
      </div>
    </a-drawer>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { scheduleApi } from '@/api/scheduleApi'
import { attendanceApi } from '@/api/attendanceApi'
import { resultApi } from '@/api/resultApi'
import { aiApi } from '@/api/aiApi'

const router = useRouter()
const route = useRoute()

const props = defineProps({ classId: { type: String, required: true } })
const loading = ref(false)
const activeTab = ref('students')
const classInfo = ref(null)
const enrollments = ref([])
const schedules = ref([])
const sessions = ref([])
const results = ref([])

// Column definition
const studentColumns = [
  { title: 'ThÃ´ng tin Há»c viÃªn', key: 'student', width: 280 }, 
  { title: 'KhÃ³a há»c', dataIndex: 'courseNameSnapshot', key: 'courseNameSnapshot' }, 
  { title: 'NgÃ y ghi danh', dataIndex: 'enrolledAt', key: 'enrolledAt', customRender: ({ text }) => formatDate(text) }, 
  { title: 'Tráº¡ng thÃ¡i', key: 'status' }
]

// Má»Ÿ rá»™ng cá»™t thá»i gian há»c Ä‘á»ƒ chá»©a cÃ¡c Badge fix width
const scheduleColumns = [
  { title: 'Buá»•i há»c', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 100, align: 'center' }, 
  { title: 'Thá»i gian há»c (Thá»© - Ca - Giá»)', key: 'time', width: 340 }, 
  { title: 'PhÃ²ng', dataIndex: 'room', key: 'room', width: 100 }, 
  { title: 'Ná»™i dung (Chá»§ Ä‘á»)', dataIndex: 'topic', key: 'topic' }
]

const sessionColumns = [
  { title: 'PhiÃªn Ä‘iá»ƒm danh', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 150, align: 'center' }, 
  { title: 'NgÃ y diá»…n ra', dataIndex: 'attendanceDate', key: 'attendanceDate', width: 150, customRender: ({ text }) => formatDate(text) }, 
  { title: 'Ná»™i dung (Chá»§ Ä‘á»)', dataIndex: 'topic', key: 'topic' }, 
  { title: 'Tráº¡ng thÃ¡i khÃ³a sá»•', key: 'status', width: 180 }
]

const resultColumns = [
  { title: 'Há»c viÃªn', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot', width: 220 }, 
  { title: 'Giá»¯a ká»³', dataIndex: 'midtermScore', key: 'midtermScore', width: 100 }, 
  { title: 'Cuá»‘i ká»³', dataIndex: 'finalScore', key: 'finalScore', width: 100 }, 
  { title: 'Tá»•ng káº¿t', key: 'averageScore', width: 100 }, 
  { title: 'Xáº¿p loáº¡i', key: 'resultStatus', width: 150 }
]

// ================= HÃ€M ÄIá»€U HÆ¯á»šNG BÃŠN NGOÃ€I (DEEP LINKING FIX) =================
function goToAttendance() {
  if (!classInfo.value?.id) return;
  router.push({
    path: '/teacher/attendance',
    query: { classId: classInfo.value.id }
  })
}

function goToResults() {
  if (!classInfo.value?.id) return;
  router.push({
    path: '/teacher/results',
    query: { classId: classInfo.value.id }
  })
}

// Formatting Helpers
function formatDate(d) {
  if (!d) return 'â€”'
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
function formatTime(t) {
  if (!t) return 'â€”'
  return String(t).substring(0, 5) // "13:30:00" -> "13:30"
}
function getInitials(name) {
  if (!name) return 'HV'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

// Avatar Colors
const AVATAR_COLORS = [
  'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)', 
  'linear-gradient(135deg, #10b981 0%, #047857 100%)', 
  'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)', 
  'linear-gradient(135deg, #8b5cf6 0%, #5b21b6 100%)', 
  'linear-gradient(135deg, #ec4899 0%, #be185d 100%)', 
  'linear-gradient(135deg, #0ea5e9 0%, #0369a1 100%)', 
]
function avatarBg(name) {
  if (!name) return 'linear-gradient(135deg, #94a3b8 0%, #475569 100%)'
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

// Label Helpers
function dayLabel(value) { return ({ 0: 'Chá»§ nháº­t', 1: 'Thá»© Hai', 2: 'Thá»© Ba', 3: 'Thá»© TÆ°', 4: 'Thá»© NÄƒm', 5: 'Thá»© SÃ¡u', 6: 'Thá»© Báº£y' }[value] || value) }
function shiftLabel(value) { return ({ 0: 'Ca SÃ¡ng', 1: 'Ca Chiá»u', 2: 'Ca Tá»‘i' }[value] || value) }

function enrollmentLabel(v) { return ({ 1: 'Chá» xÃ¡c nháº­n', 2: 'ÄÃ£ xÃ¡c nháº­n', 3: 'Äang há»c', 4: 'HoÃ n thÃ nh', 5: 'ÄÃ£ há»§y' }[v] || v) }
function enrollmentBadgeClass(v) { return ({ 1: 'bg-amber-50 text-amber-700 border-amber-200', 2: 'bg-blue-50 text-blue-700 border-blue-200', 3: 'bg-purple-50 text-purple-700 border-purple-200', 4: 'bg-emerald-50 text-emerald-700 border-emerald-200', 5: 'bg-slate-50 text-slate-600 border-slate-200' }[v] || 'bg-slate-50 text-slate-600') }

function sessionLabel(v) { return (String(v) === 'Locked' || Number(v) === 2) ? 'ÄÃ£ khÃ³a' : 'Äang má»Ÿ' }
function sessionStatusClass(v) { return (String(v) === 'Locked' || Number(v) === 2) ? 'bg-rose-50 text-rose-700 border-rose-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200' }

function resultLabel(v) { return ({ 1: 'Äang há»c', 2: 'Äáº¡t', 3: 'KhÃ´ng Ä‘áº¡t' }[v] || v) }
function resultBadgeClass(v) { return ({ 1: 'bg-blue-50 text-blue-700 border-blue-200', 2: 'bg-emerald-50 text-emerald-700 border-emerald-200', 3: 'bg-rose-50 text-rose-700 border-rose-200' }[v] || 'bg-slate-50 text-slate-600') }

function getScoreColor(score) {
  if (score == null) return 'text-slate-400'
  if (score >= 8) return 'text-emerald-600'
  if (score >= 5) return 'text-blue-600'
  return 'text-rose-600'
}

async function loadData() {
  loading.value = true
  try {
    const [cls, en, sch, ses, res] = await Promise.all([
      classApi.getById(props.classId),
      enrollmentApi.getByClass(props.classId),
      scheduleApi.getByClass(props.classId),
      attendanceApi.getSessionsByClass(props.classId),
      resultApi.getByClass(props.classId)
    ])
    classInfo.value = cls
    enrollments.value = en || []
    
    // Sort schedules by session number
    schedules.value = (sch || []).sort((a,b) => a.sessionNumber - b.sessionNumber)
    
    // Sort sessions by number
    sessions.value = (ses || []).sort((a,b) => b.sessionNumber - a.sessionNumber)
    results.value = res || []
  } finally {
    loading.value = false
  }
}

// Risk analysis clustering computed properties
const studentRiskList = computed(() => {
  if (!enrollments.value.length) return []
  
  const resultByStudent = new Map(results.value.map(r => [r.studentId, r]))
  
  return enrollments.value.map(en => {
    const res = resultByStudent.get(en.studentId)
    const midterm = res?.midtermScore != null ? Number(res.midtermScore) : null
    const final = res?.finalScore != null ? Number(res.finalScore) : null
    const attendancePercent = res?.attendancePercent != null ? Number(res.attendancePercent) : 100
    
    let avgScore = 0
    if (midterm != null && final != null) {
      avgScore = midterm * 0.4 + final * 0.6
    } else if (final != null) {
      avgScore = final
    } else if (midterm != null) {
      avgScore = midterm
    }
    
    // Risk assessment clustering
    let riskLevel = 'safe' // 'high', 'medium', 'safe'
    if (avgScore < 5.0 || attendancePercent < 80) {
      riskLevel = 'high'
    } else if (avgScore < 6.5 || attendancePercent < 90) {
      riskLevel = 'medium'
    }
    
    // Determine knowledge gaps based on course and average
    const knowledgeGaps = []
    if (attendancePercent < 90) {
      knowledgeGaps.push('Thiáº¿u giá» lÃªn lá»›p')
    }
    if (midterm != null && midterm < 5.0) {
      knowledgeGaps.push('Kiáº¿n thá»©c Giá»¯a ká»³')
    }
    if (final != null && final < 5.0) {
      knowledgeGaps.push('Ká»¹ nÄƒng Thá»±c hÃ nh Dá»± Ã¡n')
    }
    if (!knowledgeGaps.length && riskLevel !== 'safe') {
      knowledgeGaps.push('Há»•ng kiáº¿n thá»©c ná»n táº£ng')
    }
    
    return {
      studentId: en.studentId,
      studentName: en.studentNameSnapshot || 'Há»c viÃªn',
      attendancePercent,
      avgScore,
      riskLevel,
      knowledgeGaps
    }
  })
})

const riskStats = computed(() => {
  const list = studentRiskList.value
  return {
    highRiskCount: list.filter(s => s.riskLevel === 'high').length,
    mediumRiskCount: list.filter(s => s.riskLevel === 'medium').length,
    safeCount: list.filter(s => s.riskLevel === 'safe').length
  }
})

const redFlagStudents = computed(() => {
  return studentRiskList.value.filter(s => s.riskLevel === 'high')
})

// Risk Recovery AI State & Methods
const riskDrawerVisible = ref(false)
const riskAiLoading = ref(false)
const activeRiskStudent = ref(null)
const aiRiskPlanResult = ref(null)

function openRiskPlan(student) {
  activeRiskStudent.value = student
  aiRiskPlanResult.value = null
  riskDrawerVisible.value = true
}

function closeRiskPlan() {
  riskDrawerVisible.value = false
  activeRiskStudent.value = null
}

async function generateRiskPlan() {
  if (!activeRiskStudent.value) return
  riskAiLoading.value = true
  aiRiskPlanResult.value = null

  const student = activeRiskStudent.value
  const promptContext = `
HÃ£y Ä‘Ã³ng vai lÃ  giÃ¡o viÃªn cá»‘ váº¥n há»c táº­p cá»§a lá»›p "${classInfo.value?.className || 'Lá»›p há»c'}" dáº¡y mÃ´n "${classInfo.value?.courseNameSnapshot || 'KhÃ³a há»c'}".
Há»c viÃªn hiá»‡n táº¡i Ä‘ang gáº·p rá»§i ro há»c táº­p nghiÃªm trá»ng (Red Flag) vá»›i cÃ¡c thÃ´ng tin:
- TÃªn: ${student.studentName}
- Tá»· lá»‡ Ä‘i há»c: ${student.attendancePercent}%
- Äiá»ƒm trung bÃ¬nh: ${student.avgScore.toFixed(1)}/10
- Lá»— há»•ng kiáº¿n thá»©c chÃ­nh: ${student.knowledgeGaps.join(', ')}

Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  táº¡o ra:
1. Má»™t lá»™ trÃ¬nh Ã´n táº­p bá»• trá»£ ngáº¯n gá»n, chi tiáº¿t (khoáº£ng 3-4 dÃ²ng/bÆ°á»›c) chá»‰ ra chÃ­nh xÃ¡c há»c viÃªn cáº§n xem láº¡i bÃ i giáº£ng nÃ o, lÃ m bÃ i táº­p gÃ¬ vÃ  liÃªn há»‡ ai Ä‘á»ƒ Ä‘Æ°á»£c trá»£ giÃºp.
2. Má»™t báº£n tháº£o email/tin nháº¯n gá»­i trá»±c tiáº¿p cho há»c viÃªn (hoáº·c phá»¥ huynh) má»™t cÃ¡ch chuyÃªn nghiá»‡p, lá»‹ch sá»±, thá»ƒ hiá»‡n sá»± quan tÃ¢m sÃ¢u sáº¯c, nÃªu rÃµ tÃ¬nh tráº¡ng hiá»‡n táº¡i vÃ  kÃªu gá»i há»c viÃªn gáº·p giÃ¡o viÃªn sau giá» há»c Ä‘á»ƒ trao Ä‘á»•i lá»™ trÃ¬nh phá»¥c há»“i.

HÃ£y tráº£ vá» káº¿t quáº£ dÆ°á»›i dáº¡ng chuá»—i JSON cÃ³ cáº¥u trÃºc sau:
{
  "plan": "Lá»™ trÃ¬nh Ã´n táº­p bá»• trá»£ Ä‘á» xuáº¥t...",
  "email": "KÃ­nh gá»­i em/gia Ä‘Ã¬nh..."
}
  `

  try {
    const resData = await aiApi.complete({
      prompt: promptContext,
      jsonMode: true,
      maxOutputTokens: 1400
    })
    const rawText = resData.text || ''
    
    const cleanJson = JSON.parse(rawText.trim())
    aiRiskPlanResult.value = {
      plan: cleanJson.plan || 'ChÆ°a thá»ƒ táº¡o lá»™ trÃ¬nh.',
      email: cleanJson.email || 'ChÆ°a thá»ƒ táº¡o email.'
    }
  } catch (err) {
    console.error(err)
    message.error('Gáº·p lá»—i khi gá»i AI táº¡o káº¿ hoáº¡ch phá»¥c há»“i.')
    // Fallback
    aiRiskPlanResult.value = {
      plan: `BÆ°á»›c 1: Xem láº¡i toÃ n bá»™ slide vÃ  code bÃ i giáº£ng vá» "${student.knowledgeGaps.join(' vÃ  ')}".\nBÆ°á»›c 2: HoÃ n thÃ nh cÃ¡c bÃ i táº­p thá»±c hÃ nh nhá» cÃ²n thiáº¿u.\nBÆ°á»›c 3: Tham gia giá» trá»£ giÃºp (Office hours) vÃ o thá»© NÄƒm lÃºc 17h Ä‘á»ƒ Ä‘Æ°á»£c giÃ¡o viÃªn hÆ°á»›ng dáº«n trá»±c tiáº¿p.`,
      email: `ChÃ o ${student.studentName},\n\nTháº§y/cÃ´ viáº¿t thÆ° nÃ y Ä‘á»ƒ trao Ä‘á»•i vá» tÃ¬nh hÃ¬nh há»c táº­p cá»§a em lá»›p ${classInfo.value?.className}. Hiá»‡n táº¡i chuyÃªn cáº§n cá»§a em á»Ÿ má»©c ${student.attendancePercent}% vÃ  Ä‘iá»ƒm trung bÃ¬nh lÃ  ${student.avgScore.toFixed(1)}, Ä‘Ã¢y lÃ  má»©c Ä‘Ã¡ng bÃ¡o Ä‘á»™ng.\n\nTháº§y/cÃ´ Ä‘Ã£ lÃªn lá»™ trÃ¬nh Ã´n táº­p bá»• sung cho em. HÃ£y sáº¯p xáº¿p gáº·p tháº§y/cÃ´ sau giá» há»c hÃ´m tá»›i Ä‘á»ƒ chÃºng ta cÃ¹ng tháº£o luáº­n nhÃ©.\n\nChÃºc em há»c tá»‘t,\nGiáº£ng viÃªn lá»›p há»c.`
    }
  } finally {
    riskAiLoading.value = false
  }
}

function copyEmailDraft() {
  if (!aiRiskPlanResult.value?.email) return
  navigator.clipboard.writeText(aiRiskPlanResult.value.email)
  message.success('ÄÃ£ sao chÃ©p báº£n tháº£o email vÃ o bá»™ nhá»› táº¡m!')
}

onMounted(() => {
  // Äá»c params tab tá»« URL Ä‘á»ƒ má»Ÿ sáºµn Tab phÃ¹ há»£p
  if (route.query.tab) {
    activeTab.value = route.query.tab
  }
  loadData()
})
</script>

<style scoped>
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.75rem;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
.custom-tabs :deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
.custom-tabs :deep(.ant-tabs-tab) {
  padding: 12px 0 16px 0;
  font-weight: 700;
  font-size: 14px;
  color: #64748b;
}
.custom-tabs :deep(.ant-tabs-tab-active) {
  color: #2563eb !important;
}
</style>
