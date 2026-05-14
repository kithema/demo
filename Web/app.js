
let db = null;
let currentLanguage = 'ru';
let allMachines = [];
let workOrders = [];
let engineers = [];
let currentCalendarDate = new Date();

const translations = {
    ru: {
        working: 'Работает',
        broken: 'Вышел из строя',
        maintenance: 'В ремонте/на обслуживании',
        new: 'Новая',
        inProgress: 'В работе',
        completed: 'Завершена',
        cancelled: 'Отменена',
        normal: 'Обычный',
        high: 'Высокий',
        emergency: 'Авария'
    },
    en: {
        working: 'Working',
        broken: 'Out of order',
        maintenance: 'Under repair',
        new: 'New',
        inProgress: 'In Progress',
        completed: 'Completed',
        cancelled: 'Cancelled',
        normal: 'Normal',
        high: 'High',
        emergency: 'Emergency'
    }
};


function openDB() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('VendingFranchiserDB', 3);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => {
            db = request.result;
            resolve(db);
        };
        
        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            
        
            if (!db.objectStoreNames.contains('machines')) {
                const machineStore = db.createObjectStore('machines', { keyPath: 'machineId', autoIncrement: true });
                machineStore.createIndex('serialNumber', 'serialNumber', { unique: true });
                machineStore.createIndex('model', 'model');
                machineStore.createIndex('status', 'status');
            }
            
        
            if (!db.objectStoreNames.contains('engineers')) {
                const engineerStore = db.createObjectStore('engineers', { keyPath: 'engineerId', autoIncrement: true });
                engineerStore.createIndex('fullName', 'fullName');
            }
            
          
            if (!db.objectStoreNames.contains('workOrders')) {
                const orderStore = db.createObjectStore('workOrders', { keyPath: 'orderId', autoIncrement: true });
                orderStore.createIndex('machineId', 'machineId');
                orderStore.createIndex('engineerId', 'engineerId');
                orderStore.createIndex('status', 'status');
                orderStore.createIndex('scheduledDate', 'scheduledDate');
                orderStore.createIndex('priority', 'priority');
            }
            
         
            if (!db.objectStoreNames.contains('initialized')) {
                db.createObjectStore('initialized', { keyPath: 'id' });
            }
        };
    });
}


async function checkAndAddInitialData() {
    if (!db) return;
    
    const transaction = db.transaction(['initialized'], 'readonly');
    const store = transaction.objectStore('initialized');
    
    return new Promise((resolve) => {
        const request = store.get('initialized');
        request.onsuccess = () => {
            if (!request.result) {
                addInitialData();
            }
            resolve();
        };
        request.onerror = () => {

            addInitialData();
            resolve();
        };
    });
}


function addInitialData() {
    if (!db) return;
    

    const engineersData = [
        { fullName: 'Иван Петров', email: 'ivan@example.com', phone: '+7(999)111-2233', supportedModels: 'CoffeMachine,SodaMachine', maxTasksPerWeek: 15, isActive: true },
        { fullName: 'Сергей Сидоров', email: 'sergey@example.com', phone: '+7(999)444-5566', supportedModels: 'SnackMachine,CoffeMachine', maxTasksPerWeek: 15, isActive: true },
        { fullName: 'Анна Иванова', email: 'anna@example.com', phone: '+7(999)777-8899', supportedModels: 'SodaMachine,SnackMachine', maxTasksPerWeek: 15, isActive: true }
    ];
    

    const machinesData = [
        { serialNumber: 'SN-001', model: 'CoffeMachine', manufacturer: 'VendoCorp', location: 'ТЦ "Европа", 1 этаж', manufactureDate: '2023-01-15', commissioningDate: '2023-02-01', lastVerificationDate: '2025-05-01', verificationInterval: 6, nextMaintenanceDate: '2025-11-01', status: 'working' },
        { serialNumber: 'SN-002', model: 'SodaMachine', manufacturer: 'DrinkTech', location: 'БЦ "Плаза", lobby', manufactureDate: '2023-03-10', commissioningDate: '2023-04-01', lastVerificationDate: '2025-04-15', verificationInterval: 6, nextMaintenanceDate: '2025-10-15', status: 'working' },
        { serialNumber: 'SN-003', model: 'SnackMachine', manufacturer: 'SnackCorp', location: 'Школа №45', manufactureDate: '2023-05-20', commissioningDate: '2023-06-01', lastVerificationDate: '2025-03-10', verificationInterval: 6, nextMaintenanceDate: '2025-09-10', status: 'maintenance' },
        { serialNumber: 'SN-004', model: 'CoffeMachine', manufacturer: 'VendoCorp', location: 'Аэропорт', manufactureDate: '2023-07-01', commissioningDate: '2023-08-01', lastVerificationDate: '2025-06-01', verificationInterval: 6, nextMaintenanceDate: '2025-12-01', status: 'working' }
    ];
    
    const transaction = db.transaction(['engineers', 'machines', 'initialized'], 'readwrite');
    
    engineersData.forEach(e => transaction.objectStore('engineers').add(e));
    machinesData.forEach(m => transaction.objectStore('machines').add(m));
    transaction.objectStore('initialized').add({ id: 'initialized', value: true });
    
    transaction.oncomplete = () => {
        console.log('Начальные данные добавлены');

        loadMachines();
        loadEngineers();
        loadOrders();
    };
    
    transaction.onerror = (error) => {
        console.error('Ошибка добавления начальных данных:', error);
    };
}


document.addEventListener('DOMContentLoaded', async () => {
    try {
        await openDB();
        await checkAndAddInitialData(); 
        await loadMachines();
        await loadEngineers();
        await loadOrders();
        setupEventListeners();
        applyLanguage();
        console.log('Приложение готово к работе!');
    } catch (error) {
        console.error('Ошибка инициализации:', error);
        showAlert('uploadResult', 'Ошибка инициализации приложения: ' + error.message, 'error');
    }
});

function setupEventListeners() {

    document.querySelectorAll('.menu-item').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const page = btn.dataset.page;
            if (page) switchPage(page);
        });
    });
    

    document.querySelectorAll('.lang-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            currentLanguage = btn.dataset.lang;
            applyLanguage();
            refreshCurrentPage();
        });
    });
    

    const uploadZone = document.getElementById('uploadZone');
    const fileInput = document.getElementById('csvFile');
    const uploadBtn = document.getElementById('uploadBtn');
    
    if (uploadZone) {
        uploadZone.addEventListener('click', (e) => {
            if (uploadBtn && (e.target === uploadBtn || uploadBtn.contains(e.target))) {
                return;
            }
            if (fileInput) fileInput.click();
        });
    }
    
    if (uploadBtn) {
        uploadBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (fileInput) fileInput.click();
        });
    }
    
    if (fileInput) {
        fileInput.addEventListener('change', uploadCSV);
    }
    

    if (uploadZone) {
        uploadZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadZone.classList.add('drag-over');
        });
        uploadZone.addEventListener('dragleave', () => {
            uploadZone.classList.remove('drag-over');
        });
        uploadZone.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadZone.classList.remove('drag-over');
            const file = e.dataTransfer.files[0];
            if (file && file.name.endsWith('.csv')) {
                processCSV(file);
            } else {
                showAlert('uploadResult', 'Пожалуйста, выберите файл CSV', 'error');
            }
        });
    }
    
    const calendarMode = document.getElementById('calendar-mode');
    if (calendarMode) {
        calendarMode.addEventListener('change', toggleMachineSelect);
    }
    
    const refreshCalendar = document.getElementById('refresh-calendar');
    if (refreshCalendar) {
        refreshCalendar.addEventListener('click', () => loadCalendar());
    }
    
    // Заявки
    const createOrderBtn = document.getElementById('create-order');
    if (createOrderBtn) {
        createOrderBtn.addEventListener('click', createOrder);
    }
    
    const saveScheduleBtn = document.getElementById('save-schedule');
    if (saveScheduleBtn) {
        saveScheduleBtn.addEventListener('click', saveSchedule);
    }
}

function switchPage(pageName) {
    // Скрываем все страницы
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    const targetPage = document.getElementById(`page-${pageName}`);
    if (targetPage) targetPage.classList.add('active');
    
    // Обновляем активную кнопку меню
    document.querySelectorAll('.menu-item').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.page === pageName) btn.classList.add('active');
    });
    
    // Загружаем данные для выбранной страницы
    if (pageName === 'machines') {
        loadMachines();
    } else if (pageName === 'calendar') {
        loadCalendar();
    } else if (pageName === 'schedule') {
        Promise.all([loadOrders(), loadEngineers()]).then(() => {
            renderScheduleCalendar();
            renderOrdersTable();
        });
    }
}

function applyLanguage() {
    refreshCurrentPage();
}

function refreshCurrentPage() {
    const activePage = document.querySelector('.page.active');
    if (!activePage) return;
    
    if (activePage.id === 'page-machines') {
        renderMachinesTable();
    } else if (activePage.id === 'page-calendar') {
        loadCalendar();
    } else if (activePage.id === 'page-schedule') {
        renderOrdersTable();
        renderScheduleCalendar();
    }
}

// ========== РАБОТА С АППАРАТАМИ ==========
async function loadMachines() {
    return new Promise((resolve, reject) => {
        if (!db) {
            reject('База данных не инициализирована');
            return;
        }
        const transaction = db.transaction(['machines'], 'readonly');
        const store = transaction.objectStore('machines');
        const request = store.getAll();
        
        request.onsuccess = () => {
            allMachines = request.result || [];
            renderMachinesTable();
            fillMachineSelects();
            resolve(allMachines);
        };
        request.onerror = () => reject(request.error);
    });
}

function renderMachinesTable() {
    const tbody = document.getElementById('machines-list');
    if (!tbody) return;
    
    const t = translations[currentLanguage];
    tbody.innerHTML = '';
    
    if (!allMachines || allMachines.length === 0) {
        tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;">Нет данных</td></tr>';
        return;
    }
    
    allMachines.forEach(m => {
        const statusClass = getStatusClass(m.status);
        const statusText = getStatusText(m.status);
        
        tbody.innerHTML += `
            <tr>
                <td>${m.machineId || '-'}</td>
                <td><strong>${m.serialNumber || '-'}</strong></td>
                <td>${m.model || '-'}</td>
                <td>${m.manufacturer || '-'}</td>
                <td>${m.location || '-'}</td>
                <td><span class="status-badge ${statusClass}">${statusText}</span></td>
                <td>${formatDate(m.manufactureDate)}</td>
                <td>${formatDate(m.nextMaintenanceDate)}</td>
                <td>
                    <button class="small" onclick="editMachine(${m.machineId})">✏️</button>
                    <button class="small danger" onclick="deleteMachine(${m.machineId})">🗑️</button>
                </td>
            </tr>
        `;
    });
}

function getStatusClass(status) {
    if (status === 'working') return 'status-working';
    if (status === 'broken') return 'status-broken';
    if (status === 'maintenance') return 'status-maintenance';
    return '';
}

function getStatusText(status) {
    const t = translations[currentLanguage];
    if (status === 'working') return t.working;
    if (status === 'broken') return t.broken;
    if (status === 'maintenance') return t.maintenance;
    return status || '-';
}

function fillMachineSelects() {
    const selects = ['calendar-machine', 'order-machine'];
    selects.forEach(id => {
        const select = document.getElementById(id);
        if (select) {
            select.innerHTML = '<option value="">-- Выберите аппарат --</option>';
            if (allMachines && allMachines.length > 0) {
                allMachines.forEach(m => {
                    const option = document.createElement('option');
                    option.value = m.machineId;
                    option.textContent = `${m.serialNumber} - ${m.model} (${m.location || 'нет локации'})`;
                    select.appendChild(option);
                });
            }
        }
    });
}

// ========== ЗАГРУЗКА CSV ==========
function uploadCSV(event) {
    const file = event.target.files[0];
    if (!file) return;
    processCSV(file);
    event.target.value = '';
}

function processCSV(file) {
    const reader = new FileReader();
    reader.onload = function(e) {
        const content = e.target.result;
        const lines = content.split(/\r?\n/).filter(l => l.trim());
        
        if (lines.length < 2) {
            showAlert('uploadResult', 'Файл пуст или содержит только заголовки', 'error');
            return;
        }
        
        // Пропускаем заголовок (первую строку)
        const hasHeader = lines[0].toLowerCase().includes('серийный') || 
                         lines[0].toLowerCase().includes('serial') ||
                         lines[0].includes('SN');
        
        const startLine = hasHeader ? 1 : 0;
        const errors = [];
        let successCount = 0;
        
        for (let i = startLine; i < lines.length; i++) {
            const cols = parseCSVLine(lines[i]);
            if (cols.length < 2) {
                errors.push(`Строка ${i + 1}: Недостаточно данных`);
                continue;
            }
            
            const machine = {
                serialNumber: cols[0]?.trim(),
                model: cols[1]?.trim(),
                manufacturer: cols[2]?.trim() || 'Неизвестно',
                location: cols[3]?.trim() || 'Не указана',
                manufactureDate: cols[4]?.trim() || new Date().toISOString().split('T')[0],
                commissioningDate: cols[5]?.trim() || new Date().toISOString().split('T')[0],
                lastVerificationDate: cols[6]?.trim() || new Date().toISOString().split('T')[0],
                verificationInterval: parseInt(cols[7]) || 6,
                status: 'working'
            };
            
            if (!machine.serialNumber || !machine.model) {
                errors.push(`Строка ${i + 1}: Отсутствуют обязательные поля (серийный номер или модель)`);
                continue;
            }
            
            // Проверка на дубликат
            const exists = allMachines.some(m => m.serialNumber === machine.serialNumber);
            if (exists) {
                errors.push(`Строка ${i + 1}: Серийный номер ${machine.serialNumber} уже существует`);
                continue;
            }
            
            // Расчет следующего ТО
            if (machine.lastVerificationDate) {
                const lastDate = new Date(machine.lastVerificationDate);
                if (!isNaN(lastDate.getTime())) {
                    machine.nextMaintenanceDate = new Date(lastDate.setMonth(lastDate.getMonth() + machine.verificationInterval)).toISOString().split('T')[0];
                } else {
                    machine.nextMaintenanceDate = new Date().toISOString().split('T')[0];
                }
            }
            
            saveMachineToDB(machine);
            successCount++;
        }
        
        if (errors.length > 0) {
            showAlert('uploadResult', `✅ Загружено: ${successCount}\n⚠️ Ошибки:\n${errors.slice(0, 5).join('\n')}${errors.length > 5 ? `\n... и еще ${errors.length - 5}` : ''}`, 'warning');
        } else {
            showAlert('uploadResult', `✅ Успешно загружено ${successCount} записей`, 'success');
        }
        
        setTimeout(() => loadMachines(), 500);
    };
    reader.onerror = () => {
        showAlert('uploadResult', 'Ошибка чтения файла', 'error');
    };
    reader.readAsText(file, 'UTF-8');
}

function parseCSVLine(line) {
    const result = [];
    let inQuotes = false;
    let current = '';
    
    for (let i = 0; i < line.length; i++) {
        const char = line[i];
        if (char === '"') {
            if (inQuotes && line[i + 1] === '"') {
                current += '"';
                i++;
            } else {
                inQuotes = !inQuotes;
            }
        } else if (char === ',' && !inQuotes) {
            result.push(current.trim());
            current = '';
        } else {
            current += char;
        }
    }
    result.push(current.trim());
    return result;
}

function saveMachineToDB(machine) {
    if (!db) return;
    const transaction = db.transaction(['machines'], 'readwrite');
    const store = transaction.objectStore('machines');
    store.add(machine);
}

async function deleteMachine(id) {
    if (!confirm('Удалить аппарат? Это также удалит все связанные заявки.')) return;
    
    if (!db) return;
    const transaction = db.transaction(['machines', 'workOrders'], 'readwrite');
    const machineStore = transaction.objectStore('machines');
    machineStore.delete(id);
    
    // Удаляем связанные заявки
    const orderStore = transaction.objectStore('workOrders');
    const index = orderStore.index('machineId');
    const request = index.getAll(IDBKeyRange.only(id));
    request.onsuccess = () => {
        if (request.result) {
            request.result.forEach(order => {
                orderStore.delete(order.orderId);
            });
        }
    };
    
    transaction.oncomplete = () => {
        loadMachines();
        loadOrders();
        showAlert('uploadResult', 'Аппарат удален', 'success');
    };
    transaction.onerror = () => {
        showAlert('uploadResult', 'Ошибка при удалении', 'error');
    };
}

// ========== КАЛЕНДАРЬ ==========
function toggleMachineSelect() {
    const mode = document.getElementById('calendar-mode');
    const group = document.getElementById('machine-select-group');
    if (mode && group) {
        group.style.display = mode.value === 'single' ? 'block' : 'none';
    }
}

async function loadCalendar() {
    const container = document.getElementById('calendar-container');
    if (!container) return;
    
    const modeSelect = document.getElementById('calendar-mode');
    const periodSelect = document.getElementById('calendar-period');
    const machineSelect = document.getElementById('calendar-machine');
    
    if (!modeSelect || !periodSelect) return;
    
    const mode = modeSelect.value;
    const machineId = mode === 'single' && machineSelect ? parseInt(machineSelect.value) : null;
    const period = periodSelect.value;
    
    let machines = allMachines || [];
    if (machineId && !isNaN(machineId)) {
        machines = machines.filter(m => m.machineId === machineId);
    }
    
    if (period === 'week') container.innerHTML = renderWeekCalendar(machines);
    else if (period === 'month') container.innerHTML = renderMonthCalendar(machines);
    else container.innerHTML = renderYearCalendar(machines);
}

function renderWeekCalendar(machines) {
    const weekDays = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'ВС'];
    const startOfWeek = new Date(currentCalendarDate);
    const dayOfWeek = startOfWeek.getDay();
    startOfWeek.setDate(startOfWeek.getDate() - (dayOfWeek === 0 ? 6 : dayOfWeek - 1));
    
    let html = '<div class="calendar-week">';
    for (let i = 0; i < 7; i++) {
        const currentDate = new Date(startOfWeek);
        currentDate.setDate(startOfWeek.getDate() + i);
        const dateStr = currentDate.toISOString().split('T')[0];
        
        html += `<div class="calendar-day">
            <div class="calendar-day-header"><strong>${weekDays[i]}</strong><br>${currentDate.getDate()}.${currentDate.getMonth() + 1}</div>`;
        
        machines.forEach(m => {
            if (m.nextMaintenanceDate && m.nextMaintenanceDate === dateStr) {
                const colorClass = getMaintenanceColorClass(m.nextMaintenanceDate);
                html += `<div class="calendar-event ${colorClass}" onclick="showMachineInfo(${m.machineId})">
                    🔧 ${m.serialNumber} (${m.model})
                </div>`;
            }
        });
        html += '</div>';
    }
    html += '</div>';
    return html;
}

function renderMonthCalendar(machines) {
    const year = currentCalendarDate.getFullYear();
    const month = currentCalendarDate.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstDay = new Date(year, month, 1).getDay();
    const startOffset = firstDay === 0 ? 6 : firstDay - 1;
    const monthNames = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];
    
    let html = `<div style="margin-bottom: 20px; text-align: center;">
        <div class="calendar-nav">
            <button onclick="changeMonth(-1)">◀ Предыдущий</button>
            <h3>${monthNames[month]} ${year}</h3>
            <button onclick="changeMonth(1)">Следующий ▶</button>
        </div>
    </div>`;
    
    html += '<div class="month-grid">';
    const weekDays = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'ВС'];
    weekDays.forEach(d => html += `<div class="month-day"><strong>${d}</strong></div>`);
    
    for (let i = 0; i < startOffset; i++) html += '<div class="month-day"></div>';
    
    for (let d = 1; d <= daysInMonth; d++) {
        let hasEvent = false;
        let eventColor = '';
        machines.forEach(m => {
            if (m.nextMaintenanceDate) {
                const date = new Date(m.nextMaintenanceDate);
                if (!isNaN(date.getTime()) && date.getDate() === d && date.getMonth() === month && date.getFullYear() === year) {
                    hasEvent = true;
                    eventColor = getMaintenanceColorClass(m.nextMaintenanceDate);
                }
            }
        });
        html += `<div class="month-day ${hasEvent ? 'has-event ' + eventColor : ''}" onclick="showDayEvents(${year}, ${month}, ${d})">
            ${d}
        </div>`;
    }
    html += '</div>';
    return html;
}

function renderYearCalendar(machines) {
    const months = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];
    const year = currentCalendarDate.getFullYear();
    
    let html = `<div style="margin-bottom: 20px; text-align: center;">
        <div class="calendar-nav">
            <button onclick="changeYear(-1)">◀ ${year - 1}</button>
            <h2>${year}</h2>
            <button onclick="changeYear(1)">${year + 1} ▶</button>
        </div>
    </div>`;
    
    html += '<div class="year-grid">';
    
    for (let month = 0; month < 12; month++) {
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        html += `<div class="month-card">
            <div class="month-title">${months[month]}</div>
            <div class="month-days">`;
        
        const weekDays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];
        weekDays.forEach(d => html += `<div style="font-size: 9px; text-align: center; color: #999;">${d}</div>`);
        
        for (let d = 1; d <= daysInMonth; d++) {
            let hasEvent = false;
            let eventColor = '';
            machines.forEach(m => {
                if (m.nextMaintenanceDate) {
                    const date = new Date(m.nextMaintenanceDate);
                    if (!isNaN(date.getTime()) && date.getDate() === d && date.getMonth() === month && date.getFullYear() === year) {
                        hasEvent = true;
                        eventColor = getMaintenanceColorClass(m.nextMaintenanceDate);
                    }
                }
            });
            html += `<div class="month-day-small ${hasEvent ? 'event ' + eventColor : ''}" title="${hasEvent ? 'ТО запланировано' : ''}">${d}</div>`;
        }
        html += `</div></div>`;
    }
    html += '</div>';
    return html;
}

function getMaintenanceColorClass(dateStr) {
    if (!dateStr) return '';
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const maintDate = new Date(dateStr);
    maintDate.setHours(0, 0, 0, 0);
    
    if (isNaN(maintDate.getTime())) return '';
    
    const diffDays = Math.ceil((maintDate - today) / (1000 * 60 * 60 * 24));
    if (diffDays < 0) return 'red';
    if (diffDays < 5) return 'yellow';
    return 'green';
}

function changeMonth(delta) {
    currentCalendarDate.setMonth(currentCalendarDate.getMonth() + delta);
    loadCalendar();
}

function changeYear(delta) {
    currentCalendarDate.setFullYear(currentCalendarDate.getFullYear() + delta);
    loadCalendar();
}

function showDayEvents(year, month, day) {
    const events = (allMachines || []).filter(m => {
        if (!m.nextMaintenanceDate) return false;
        const date = new Date(m.nextMaintenanceDate);
        return !isNaN(date.getTime()) && date.getFullYear() === year && date.getMonth() === month && date.getDate() === day;
    });
    
    if (events.length > 0) {
        let msg = `📅 ТО на ${day}.${month + 1}.${year}:\n\n`;
        events.forEach(e => {
            msg += `🔧 ${e.serialNumber} - ${e.model}\n   📍 ${e.location}\n   📅 ${formatDate(e.nextMaintenanceDate)}\n\n`;
        });
        alert(msg);
    } else {
        alert(`На ${day}.${month + 1}.${year} нет запланированного ТО`);
    }
}

function showMachineInfo(id) {
    const machine = (allMachines || []).find(m => m.machineId === id);
    if (machine) {
        alert(`📊 ИНФОРМАЦИЯ ОБ АППАРАТЕ\n\n` +
            `Модель: ${machine.model}\n` +
            `Серийный номер: ${machine.serialNumber}\n` +
            `Производитель: ${machine.manufacturer}\n` +
            `Локация: ${machine.location}\n` +
            `Статус: ${getStatusText(machine.status)}\n` +
            `Дата производства: ${formatDate(machine.manufactureDate)}\n` +
            `Дата ввода в эксплуатацию: ${formatDate(machine.commissioningDate)}\n` +
            `Последнее ТО: ${formatDate(machine.lastVerificationDate)}\n` +
            `Следующее ТО: ${formatDate(machine.nextMaintenanceDate)}`);
    }
}

// ========== РАБОТА С ИНЖЕНЕРАМИ ==========
async function loadEngineers() {
    return new Promise((resolve, reject) => {
        if (!db) {
            reject('База данных не инициализирована');
            return;
        }
        const transaction = db.transaction(['engineers'], 'readonly');
        const store = transaction.objectStore('engineers');
        const request = store.getAll();
        
        request.onsuccess = () => {
            engineers = request.result || [];
            resolve(engineers);
        };
        request.onerror = () => reject(request.error);
    });
}

// ========== РАБОТА С ЗАЯВКАМИ ==========
async function loadOrders() {
    return new Promise((resolve, reject) => {
        if (!db) {
            reject('База данных не инициализирована');
            return;
        }
        const transaction = db.transaction(['workOrders'], 'readonly');
        const store = transaction.objectStore('workOrders');
        const request = store.getAll();
        
        request.onsuccess = () => {
            workOrders = request.result || [];
            renderOrdersTable();
            renderScheduleCalendar();
            resolve(workOrders);
        };
        request.onerror = () => reject(request.error);
    });
}

function renderOrdersTable() {
    const tbody = document.getElementById('orders-list');
    if (!tbody) return;
    
    const t = translations[currentLanguage];
    tbody.innerHTML = '';
    
    if (!workOrders || workOrders.length === 0) {
        tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;">Нет заявок</td></tr>';
        return;
    }
    
    workOrders.forEach(order => {
        const machine = (allMachines || []).find(m => m.machineId === order.machineId);
        const engineer = (engineers || []).find(e => e.engineerId === order.engineerId);
        const priorityIcon = order.priority === 'emergency' ? '🔴' : (order.priority === 'high' ? '🟠' : '🟢');
        const priorityText = getPriorityText(order.priority);
        
        tbody.innerHTML += `
            <tr>
                <td>${order.orderId || '-'}</td>
                <td>${machine?.serialNumber || '-'}</td>
                <td>${machine?.model || '-'}</td>
                <td>${order.title || '-'}</td>
                <td>${priorityIcon} ${priorityText}</td>
                <td>
                    <select class="status-select" data-id="${order.orderId}">
                        <option value="new" ${order.status === 'new' ? 'selected' : ''}>🆕 ${t.new}</option>
                        <option value="in_progress" ${order.status === 'in_progress' ? 'selected' : ''}>⚙️ ${t.inProgress}</option>
                        <option value="completed" ${order.status === 'completed' ? 'selected' : ''}>✅ ${t.completed}</option>
                        <option value="cancelled" ${order.status === 'cancelled' ? 'selected' : ''}>❌ ${t.cancelled}</option>
                    </select>
                </td>
                <td>
                    <select class="engineer-select" data-id="${order.orderId}">
                        <option value="">-- Назначить --</option>
                        ${(engineers || []).map(e => `<option value="${e.engineerId}" ${order.engineerId === e.engineerId ? 'selected' : ''}>${e.fullName}</option>`).join('')}
                    </select>
                </td>
                <td><input type="date" class="date-input" data-id="${order.orderId}" value="${order.scheduledDate || ''}"></td>
                <td>
                    <button class="small" onclick="editOrder(${order.orderId})">✏️</button>
                    <button class="small danger" onclick="deleteOrder(${order.orderId})">🗑️</button>
                  </td>
            </tr>
        `;
    });
    
    // Добавляем обработчики
    document.querySelectorAll('.status-select').forEach(select => {
        select.addEventListener('change', (e) => updateOrderStatus(e.target.dataset.id, e.target.value));
    });
    document.querySelectorAll('.engineer-select').forEach(select => {
        select.addEventListener('change', (e) => assignEngineer(e.target.dataset.id, e.target.value));
    });
    document.querySelectorAll('.date-input').forEach(input => {
        input.addEventListener('change', (e) => updateOrderDate(e.target.dataset.id, e.target.value));
    });
}

function getPriorityText(priority) {
    const t = translations[currentLanguage];
    if (priority === 'normal') return t.normal;
    if (priority === 'high') return t.high;
    if (priority === 'emergency') return t.emergency;
    return priority || 'normal';
}

function getStatusTextForOrder(status) {
    const t = translations[currentLanguage];
    if (status === 'new') return t.new;
    if (status === 'in_progress') return t.inProgress;
    if (status === 'completed') return t.completed;
    if (status === 'cancelled') return t.cancelled;
    return status || 'new';
}

function renderScheduleCalendar() {
    const container = document.getElementById('schedule-container');
    if (!container) return;
    
    const weekDays = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота', 'Воскресенье'];
    const startOfWeek = new Date();
    startOfWeek.setDate(startOfWeek.getDate() - startOfWeek.getDay() + 1);
    
    let html = '<div class="calendar-week">';
    for (let i = 0; i < 7; i++) {
        const currentDate = new Date(startOfWeek);
        currentDate.setDate(startOfWeek.getDate() + i);
        const dateStr = currentDate.toISOString().split('T')[0];
        const dayOrders = (workOrders || []).filter(o => o.scheduledDate === dateStr);
        
       
        const engineerTasks = new Map();
        dayOrders.forEach(o => {
            if (o.engineerId) {
                engineerTasks.set(o.engineerId, (engineerTasks.get(o.engineerId) || 0) + 1);
            }
        });
        const overloaded = Array.from(engineerTasks.values()).some(count => count > 4);
        
        html += `<div class="calendar-day" style="${overloaded ? 'background: #ffe6e6;' : ''}">
            <div class="calendar-day-header">
                <strong>${weekDays[i]}</strong><br>
                ${currentDate.getDate()}.${currentDate.getMonth() + 1}
            </div>`;
        
        dayOrders.forEach(order => {
            const machine = (allMachines || []).find(m => m.machineId === order.machineId);
            const engineer = (engineers || []).find(e => e.engineerId === order.engineerId);
            const priorityClass = order.priority === 'emergency' ? 'emergency' : 
                                 (order.priority === 'high' ? 'yellow' : 'green');
            
            html += `<div class="calendar-event ${priorityClass}" onclick="showOrderDetails(${order.orderId})">
                <strong>${order.title?.substring(0, 20) || 'Без названия'}</strong><br>
                <small>📍 ${machine?.location?.substring(0, 15) || '?'}</small><br>
                <small>👤 ${engineer?.fullName?.split(' ')[0] || 'не назначен'}</small>
            </div>`;
        });
        
        if (overloaded) {
            html += `<div class="warning-badge">⚠️ Перегрузка! >4 задач</div>`;
        }
        
        html += '</div>';
    }
    html += '</div>';
    container.innerHTML = html;
}


async function canAssignEngineer(engineerId, scheduledDate, currentTasks = 0) {
    if (!engineerId || !scheduledDate) return false;

    const dayTasks = (workOrders || []).filter(o => 
        o.engineerId === engineerId && 
        o.scheduledDate === scheduledDate &&
        o.status !== 'cancelled'
    ).length;
    
    if (dayTasks + currentTasks >= 4) return false;
    
   
    const date = new Date(scheduledDate);
    if (isNaN(date.getTime())) return false;
    
    const startOfWeek = new Date(date);
    startOfWeek.setDate(date.getDate() - date.getDay() + 1);
    const endOfWeek = new Date(startOfWeek);
    endOfWeek.setDate(startOfWeek.getDate() + 6);
    
    const weekTasks = (workOrders || []).filter(o => 
        o.engineerId === engineerId && 
        o.scheduledDate && 
        new Date(o.scheduledDate) >= startOfWeek &&
        new Date(o.scheduledDate) <= endOfWeek &&
        o.status !== 'cancelled'
    ).length;
    
    const engineer = (engineers || []).find(e => e.engineerId === engineerId);
    const maxPerWeek = engineer?.maxTasksPerWeek || 15;
    
    return weekTasks + currentTasks < maxPerWeek;
}


async function autoAssignEngineer(machineId, scheduledDate) {
    const machine = (allMachines || []).find(m => m.machineId === machineId);
    if (!machine) return null;
    

    let availableEngineers = (engineers || []).filter(e => 
        e.isActive && e.supportedModels && e.supportedModels.includes(machine.model)
    );
    
  
    const available = [];
    for (const eng of availableEngineers) {
        if (await canAssignEngineer(eng.engineerId, scheduledDate)) {
            available.push(eng);
        }
    }
    
    if (available.length === 0) return null;
    
  
    let bestEngineer = available[0];
    let minLoad = (workOrders || []).filter(o => 
        o.engineerId === bestEngineer.engineerId && 
        o.scheduledDate === scheduledDate
    ).length;
    
    for (const eng of available) {
        const load = (workOrders || []).filter(o => 
            o.engineerId === eng.engineerId && 
            o.scheduledDate === scheduledDate
        ).length;
        if (load < minLoad) {
            minLoad = load;
            bestEngineer = eng;
        }
    }
    
    return bestEngineer.engineerId;
}

async function createOrder() {
    const machineSelect = document.getElementById('order-machine');
    const titleInput = document.getElementById('order-title');
    const descInput = document.getElementById('order-desc');
    const prioritySelect = document.getElementById('order-priority');
    const dateInput = document.getElementById('order-date');
    
    const machineId = machineSelect ? parseInt(machineSelect.value) : NaN;
    const title = titleInput?.value || '';
    const description = descInput?.value || '';
    const priority = prioritySelect?.value || 'normal';
    let scheduledDate = dateInput?.value || '';
    
    if (isNaN(machineId)) {
        showAlert('', 'Выберите аппарат', 'error');
        return;
    }
    if (!title) {
        showAlert('', 'Введите заголовок', 'error');
        return;
    }
    
    if (!scheduledDate) {
        scheduledDate = new Date();
        scheduledDate.setDate(scheduledDate.getDate() + 3);
        scheduledDate = scheduledDate.toISOString().split('T')[0];
    }
    

    let engineerId = null;
    if (priority !== 'emergency') {
        engineerId = await autoAssignEngineer(machineId, scheduledDate);
        
        if (!engineerId) {
            alert('❌ Нет доступных сотрудников, которые могут обслуживать данный аппарат');
            return;
        }
    } else {
     
        scheduledDate = new Date().toISOString().split('T')[0];
        engineerId = await autoAssignEngineer(machineId, scheduledDate);
    }
    
    const newOrder = {
        machineId,
        engineerId,
        title,
        description,
        priority,
        status: 'new',
        scheduledDate,
        createdAt: new Date().toISOString()
    };
    
    if (priority === 'emergency' && engineerId) {
        
        const engineerOrders = (workOrders || []).filter(o => 
            o.engineerId === engineerId && 
            o.scheduledDate === scheduledDate &&
            o.status !== 'cancelled'
        );
        
        for (const order of engineerOrders) {
            const newDate = new Date(order.scheduledDate);
            newDate.setDate(newDate.getDate() + 1);
            await updateOrderDate(order.orderId, newDate.toISOString().split('T')[0]);
        }
    }
    
  
    if (!db) {
        alert('❌ База данных не инициализирована');
        return;
    }
    
    const transaction = db.transaction(['workOrders'], 'readwrite');
    const store = transaction.objectStore('workOrders');
    const request = store.add(newOrder);
    
    request.onsuccess = () => {
        showAlert('', '✅ Заявка успешно создана', 'success');
        if (titleInput) titleInput.value = '';
        if (descInput) descInput.value = '';
        loadOrders();
    };
    
    request.onerror = () => {
        alert('❌ Ошибка при создании заявки');
    };
}

async function updateOrderStatus(id, status) {
    if (!db) return;
    const transaction = db.transaction(['workOrders'], 'readwrite');
    const store = transaction.objectStore('workOrders');
    const order = await getOrderById(id);
    
    if (order) {
        order.status = status;
        if (status === 'completed') {
            order.completedAt = new Date().toISOString();
        }
        store.put(order);
        transaction.oncomplete = () => loadOrders();
    }
}

async function assignEngineer(id, engineerId) {
    if (!engineerId) return;
    if (!db) return;
    
    const order = await getOrderById(id);
    if (!order) return;
    
    if (order.scheduledDate) {
        const canAssign = await canAssignEngineer(parseInt(engineerId), order.scheduledDate);
        if (!canAssign) {
            alert('⚠️ Перегрузка сотрудника (> 4 задач/день или > 15 в неделю)');
            return;
        }
    }
    
    const transaction = db.transaction(['workOrders'], 'readwrite');
    const store = transaction.objectStore('workOrders');
    order.engineerId = parseInt(engineerId);
    store.put(order);
    transaction.oncomplete = () => loadOrders();
}

async function updateOrderDate(id, date) {
    if (!db) return;
    const transaction = db.transaction(['workOrders'], 'readwrite');
    const store = transaction.objectStore('workOrders');
    const order = await getOrderById(id);
    
    if (order) {
        order.scheduledDate = date;
        store.put(order);
        transaction.oncomplete = () => loadOrders();
    }
}

function getOrderById(id) {
    return new Promise((resolve, reject) => {
        if (!db) {
            reject('База данных не инициализирована');
            return;
        }
        const transaction = db.transaction(['workOrders'], 'readonly');
        const store = transaction.objectStore('workOrders');
        const request = store.get(parseInt(id));
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function deleteOrder(id) {
    if (!confirm('Удалить заявку?')) return;
    if (!db) return;
    
    const transaction = db.transaction(['workOrders'], 'readwrite');
    const store = transaction.objectStore('workOrders');
    store.delete(parseInt(id));
    transaction.oncomplete = () => {
        loadOrders();
        showAlert('', 'Заявка удалена', 'success');
    };
}

async function saveSchedule() {
    if (!db) {
        alert('❌ База данных не инициализирована');
        return;
    }
    
    const transaction = db.transaction(['workOrders', 'machines'], 'readwrite');
    const orderStore = transaction.objectStore('workOrders');
    const machineStore = transaction.objectStore('machines');
    
    const orders = await new Promise((resolve) => {
        const req = orderStore.getAll();
        req.onsuccess = () => resolve(req.result || []);
    });
    
    for (const order of orders) {
        if (order.status === 'new' && order.engineerId) {
            order.status = 'in_progress';
            orderStore.put(order);
            
            const machine = await new Promise((resolve) => {
                const req = machineStore.get(order.machineId);
                req.onsuccess = () => resolve(req.result);
            });
            
            if (machine) {
                machine.status = 'maintenance';
                machineStore.put(machine);
            }
        }
    }
    
    transaction.oncomplete = () => {
        alert('✅ Расписание сохранено! Статусы заявок и оборудования обновлены.');
        loadMachines();
        loadOrders();
    };
    transaction.onerror = () => {
        alert('❌ Ошибка при сохранении');
    };
}

function showOrderDetails(id) {
    const order = (workOrders || []).find(o => o.orderId === parseInt(id));
    if (order) {
        const machine = (allMachines || []).find(m => m.machineId === order.machineId);
        const engineer = (engineers || []).find(e => e.engineerId === order.engineerId);
        alert(`📋 ИНФОРМАЦИЯ О ЗАЯВКЕ #${order.orderId}\n\n` +
            `Заголовок: ${order.title}\n` +
            `Описание: ${order.description || '—'}\n` +
            `Приоритет: ${getPriorityText(order.priority)}\n` +
            `Статус: ${getStatusTextForOrder(order.status)}\n` +
            `Аппарат: ${machine?.serialNumber} (${machine?.model})\n` +
            `Локация: ${machine?.location}\n` +
            `Сотрудник: ${engineer?.fullName || 'не назначен'}\n` +
            `Дата выполнения: ${formatDate(order.scheduledDate)}\n` +
            `Создана: ${formatDate(order.createdAt)}`);
    }
}

function editMachine(id) {
    const machine = (allMachines || []).find(m => m.machineId === id);
    if (machine) {
        const newLocation = prompt('Введите новую локацию:', machine.location);
        if (newLocation && newLocation !== machine.location) {
            machine.location = newLocation;
            if (!db) return;
            const transaction = db.transaction(['machines'], 'readwrite');
            transaction.objectStore('machines').put(machine);
            transaction.oncomplete = () => loadMachines();
        }
    }
}

function editOrder(id) {
    const order = (workOrders || []).find(o => o.orderId === id);
    if (order) {
        const newTitle = prompt('Введите новый заголовок:', order.title);
        if (newTitle && newTitle !== order.title) {
            order.title = newTitle;
            if (!db) return;
            const transaction = db.transaction(['workOrders'], 'readwrite');
            transaction.objectStore('workOrders').put(order);
            transaction.oncomplete = () => loadOrders();
        }
    }
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    return `${d.getDate().toString().padStart(2, '0')}.${(d.getMonth() + 1).toString().padStart(2, '0')}.${d.getFullYear()}`;
}

function showAlert(containerId, message, type) {
    const container = document.getElementById(containerId) || document.body;
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type}`;
    alertDiv.textContent = message;
    container.appendChild(alertDiv);
    setTimeout(() => alertDiv.remove(), 5000);
}

window.changeMonth = changeMonth;
window.changeYear = changeYear;
window.showDayEvents = showDayEvents;
window.showMachineInfo = showMachineInfo;
window.showOrderDetails = showOrderDetails;
window.deleteMachine = deleteMachine;
window.deleteOrder = deleteOrder;
window.editMachine = editMachine;
window.editOrder = editOrder;