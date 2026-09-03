document.addEventListener('DOMContentLoaded', () => {
    // Elements
    const dropzone = document.getElementById('dropzone');
    const csvFileInput = document.getElementById('csvFileInput');
    const fileSelectText = document.getElementById('fileSelectText');
    const selectedFileInfo = document.getElementById('selectedFileInfo');
    const fileNameDisplay = document.getElementById('fileNameDisplay');
    const btnClearFile = document.getElementById('btnClearFile');
    const uploadForm = document.getElementById('uploadForm');
    const btnGeneratePlan = document.getElementById('btnGeneratePlan');
    const btnDownloadSample = document.getElementById('btnDownloadSample');
    const btnLoadSampleStandard = document.getElementById('btnLoadSampleStandard');
    const btnLoadSampleTabular = document.getElementById('btnLoadSampleTabular');

    const resultsSection = document.getElementById('resultsSection');
    const statDrones = document.getElementById('statDrones');
    const statTrips = document.getElementById('statTrips');
    const statPackages = document.getElementById('statPackages');
    const statWeight = document.getElementById('statWeight');
    const statUtilization = document.getElementById('statUtilization');
    const statExecutionTime = document.getElementById('statExecutionTime');
    const planMetadata = document.getElementById('planMetadata');
    const dronePlansContainer = document.getElementById('dronePlansContainer');

    const unassignedBanner = document.getElementById('unassignedBanner');
    const unassignedTitle = document.getElementById('unassignedTitle');
    const unassignedDescription = document.getElementById('unassignedDescription');
    const unassignedList = document.getElementById('unassignedList');

    const historyModal = document.getElementById('historyModal');
    const btnOpenHistory = document.getElementById('btnOpenHistory');
    const btnCloseHistory = document.getElementById('btnCloseHistory');
    const historyListContainer = document.getElementById('historyListContainer');

    let currentSelectedFile = null;
    let presetCsvContent = null;

    // Dropzone Events
    dropzone.addEventListener('click', (e) => {
        if (e.target.closest('#btnClearFile')) return;
        csvFileInput.click();
    });

    csvFileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
            handleFileSelect(e.target.files[0]);
        }
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropzone.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
            dropzone.classList.add('dragover');
        }, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropzone.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
            dropzone.classList.remove('dragover');
        }, false);
    });

    dropzone.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        const files = dt.files;
        if (files.length > 0) {
            handleFileSelect(files[0]);
        }
    });

    btnClearFile.addEventListener('click', (e) => {
        e.stopPropagation();
        currentSelectedFile = null;
        presetCsvContent = null;
        csvFileInput.value = '';
        selectedFileInfo.classList.add('hidden');
        fileSelectText.classList.remove('hidden');
    });

    function handleFileSelect(file) {
        if (!file.name.endsWith('.csv')) {
            alert('Please select a valid CSV file.');
            return;
        }
        currentSelectedFile = file;
        presetCsvContent = null;
        fileNameDisplay.textContent = file.name;
        selectedFileInfo.classList.remove('hidden');
        fileSelectText.classList.add('hidden');
    }

    // Preset Sample Loading
    btnLoadSampleStandard.addEventListener('click', async () => {
        try {
            const res = await fetch('/api/delivery/sample-csv?format=standard');
            presetCsvContent = await res.text();
            currentSelectedFile = null;
            fileNameDisplay.textContent = 'sample_drone_delivery.csv (Standard Preset)';
            selectedFileInfo.classList.remove('hidden');
            fileSelectText.classList.add('hidden');
        } catch (err) {
            console.error('Failed to load standard sample', err);
        }
    });

    btnLoadSampleTabular.addEventListener('click', async () => {
        try {
            const res = await fetch('/api/delivery/sample-csv?format=tabular');
            presetCsvContent = await res.text();
            currentSelectedFile = null;
            fileNameDisplay.textContent = 'sample_tabular_delivery.csv (Tabular Preset)';
            selectedFileInfo.classList.remove('hidden');
            fileSelectText.classList.add('hidden');
        } catch (err) {
            console.error('Failed to load tabular sample', err);
        }
    });

    btnDownloadSample.addEventListener('click', () => {
        window.location.href = '/api/delivery/sample-csv?format=standard';
    });

    // Form Submission & Planning Execution
    uploadForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        if (!currentSelectedFile && !presetCsvContent) {
            // Auto load sample standard if nothing selected
            const res = await fetch('/api/delivery/sample-csv?format=standard');
            presetCsvContent = await res.text();
        }

        const formData = new FormData();
        if (currentSelectedFile) {
            formData.append('file', currentSelectedFile);
        } else if (presetCsvContent) {
            formData.append('rawCsvContent', presetCsvContent);
        }

        btnGeneratePlan.disabled = true;
        btnGeneratePlan.innerHTML = '<i class="fa-solid fa-spinner animate-spin"></i><span>Generating Optimal Routes...</span>';

        try {
            const response = await fetch('/api/delivery/plan', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errData = await response.json();
                throw new Error(errData.message || 'Failed to process delivery plan.');
            }

            const plan = await response.json();
            renderDeliveryPlan(plan);

            // Scroll to results smoothly
            resultsSection.scrollIntoView({ behavior: 'smooth' });

        } catch (err) {
            alert(`Error generating plan: ${err.message}`);
        } finally {
            btnGeneratePlan.disabled = false;
            btnGeneratePlan.innerHTML = '<i class="fa-solid fa-wand-magic-sparkles"></i><span>Generate Delivery Plan</span>';
        }
    });

    // Render Delivery Plan
    function renderDeliveryPlan(plan) {
        resultsSection.classList.remove('hidden');

        statDrones.textContent = plan.dronePlans ? plan.dronePlans.length : 0;
        statTrips.textContent = plan.totalTrips || 0;
        statPackages.textContent = plan.deliveredPackagesCount || 0;
        statWeight.textContent = `${(plan.totalWeightDelivered || 0).toLocaleString()} kg`;
        statUtilization.textContent = `${plan.overallCapacityUtilizationPercentage || 0}%`;
        statExecutionTime.textContent = `${plan.executionTimeMs || 0} ms`;
        planMetadata.textContent = `Plan ID: ${plan.id} | ${new Date(plan.createdAt).toLocaleString()}`;

        // Unassigned packages banner
        if (plan.unassignedPackages && plan.unassignedPackages.length > 0) {
            unassignedBanner.classList.remove('hidden');
            unassignedTitle.textContent = `${plan.unassignedPackages.length} Package(s) Could Not Be Delivered`;
            unassignedDescription.textContent = `The following packages exceed available drone capacities or remaining trip limits:`;
            unassignedList.innerHTML = plan.unassignedPackages.map(u => `
                <span class="px-2 py-1 rounded bg-rose-900/60 border border-rose-700/60 text-[11px] font-mono">
                    ${u.package.location} (${u.package.weight}kg): ${u.reason}
                </span>
            `).join('');
        } else {
            unassignedBanner.classList.add('hidden');
        }

        // Render Drone Plans Grid
        if (!plan.dronePlans || plan.dronePlans.length === 0) {
            dronePlansContainer.innerHTML = '<div class="col-span-full text-center py-12 text-slate-500">No active trips scheduled.</div>';
            return;
        }

        dronePlansContainer.innerHTML = plan.dronePlans.map(dp => {
            const tripsHtml = dp.trips.map(trip => {
                const pkgPills = trip.packages.map(p => `
                    <span class="px-2 py-0.5 rounded bg-slate-800 text-slate-200 border border-slate-700 text-[11px] flex items-center space-x-1">
                        <span>${p.location}</span>
                        <span class="text-indigo-400 font-semibold">${p.weight}kg</span>
                    </span>
                `).join('');

                const utilPercent = trip.capacityUtilizationPercentage || 0;
                let progressColor = 'bg-emerald-500';
                if (utilPercent > 90) progressColor = 'bg-indigo-500';
                else if (utilPercent < 50) progressColor = 'bg-amber-500';

                return `
                    <div class="p-3.5 rounded-xl bg-slate-950/60 border border-slate-800/80 space-y-2">
                        <div class="flex items-center justify-between text-xs">
                            <div class="font-semibold text-white flex items-center space-x-1.5">
                                <span class="w-5 h-5 rounded-full bg-indigo-500/20 text-indigo-300 flex items-center justify-center text-[10px] font-bold">${trip.tripNumber}</span>
                                <span>Trip #${trip.tripNumber}</span>
                            </div>
                            <span class="text-slate-400 text-[11px]">Primary: <strong class="text-cyan-400">${trip.primaryLocation}</strong></span>
                        </div>

                        <!-- Capacity Bar -->
                        <div class="space-y-1">
                            <div class="flex justify-between text-[11px] text-slate-400">
                                <span>Payload: ${trip.totalWeight} / ${trip.capacity} kg</span>
                                <span class="font-bold text-slate-200">${utilPercent}%</span>
                            </div>
                            <div class="w-full h-1.5 rounded-full bg-slate-800 overflow-hidden">
                                <div class="h-full ${progressColor} transition-all duration-500" style="width: ${Math.min(utilPercent, 100)}%"></div>
                            </div>
                        </div>

                        <!-- Packages list -->
                        <div class="flex flex-wrap gap-1.5 pt-1">
                            ${pkgPills}
                        </div>
                    </div>
                `;
            }).join('');

            return `
                <div class="drone-card glass-card p-5 rounded-2xl border border-slate-800/90 bg-slate-900/60 flex flex-col justify-between space-y-4">
                    <div class="space-y-3">
                        <div class="flex items-center justify-between">
                            <div class="flex items-center space-x-3">
                                <div class="w-10 h-10 rounded-xl bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center">
                                    <i class="fa-solid fa-helicopter text-indigo-400 text-lg"></i>
                                </div>
                                <div>
                                    <h4 class="text-base font-bold text-white">${dp.droneName}</h4>
                                    <p class="text-xs text-slate-400">Capacity: <span class="text-emerald-400 font-semibold">${dp.capacity} kg</span></p>
                                </div>
                            </div>
                            <span class="px-2.5 py-1 rounded-full bg-indigo-500/10 text-indigo-300 text-xs font-semibold border border-indigo-500/20">
                                ${dp.totalTrips} ${dp.totalTrips === 1 ? 'Trip' : 'Trips'}
                            </span>
                        </div>

                        <div class="border-t border-slate-800/80 pt-3 space-y-3">
                            ${tripsHtml}
                        </div>
                    </div>

                    <div class="pt-2 text-xs text-slate-400 border-t border-slate-800/60 flex justify-between">
                        <span>Delivered Weight:</span>
                        <span class="font-bold text-white">${dp.totalWeightDelivered} kg</span>
                    </div>
                </div>
            `;
        }).join('');
    }

    // History Modal Management
    btnOpenHistory.addEventListener('click', async () => {
        historyModal.classList.remove('hidden');
        await loadHistoryPlans();
    });

    btnCloseHistory.addEventListener('click', () => {
        historyModal.classList.add('hidden');
    });

    historyModal.addEventListener('click', (e) => {
        if (e.target === historyModal) {
            historyModal.classList.add('hidden');
        }
    });

    async function loadHistoryPlans() {
        historyListContainer.innerHTML = '<div class="text-center py-8 text-slate-400"><i class="fa-solid fa-spinner animate-spin text-xl mb-2"></i><div>Loading recent plans...</div></div>';
        try {
            const res = await fetch('/api/delivery/plans');
            if (!res.ok) throw new Error('Failed to load history');
            const plans = await res.json();

            if (!plans || plans.length === 0) {
                historyListContainer.innerHTML = '<div class="text-center py-8 text-slate-500">No past plans found in database.</div>';
                return;
            }

            historyListContainer.innerHTML = plans.map(p => `
                <div class="p-4 rounded-xl bg-slate-950/60 border border-slate-800/80 hover:border-indigo-500/50 transition flex items-center justify-between">
                    <div class="space-y-1">
                        <div class="flex items-center space-x-2">
                            <span class="font-bold text-white text-sm">${p.fileName || 'delivery_plan.csv'}</span>
                            <span class="text-[11px] font-mono text-slate-400">ID: ${p.id.substring(0, 8)}...</span>
                        </div>
                        <div class="text-xs text-slate-400 flex items-center space-x-4">
                            <span><i class="fa-solid fa-helicopter text-indigo-400 mr-1"></i>${p.totalDrones || 0} Drones</span>
                            <span><i class="fa-solid fa-route text-cyan-400 mr-1"></i>${p.totalTrips || 0} Trips</span>
                            <span><i class="fa-solid fa-box text-emerald-400 mr-1"></i>${p.deliveredPackagesCount || 0} Packages</span>
                            <span><i class="fa-solid fa-clock text-slate-500 mr-1"></i>${new Date(p.createdAt).toLocaleDateString()} ${new Date(p.createdAt).toLocaleTimeString()}</span>
                        </div>
                    </div>

                    <div class="flex items-center space-x-2">
                        <button onclick="viewHistoricalPlan('${p.id}')" class="px-3 py-1.5 rounded-lg bg-indigo-600/20 hover:bg-indigo-600/40 text-indigo-300 border border-indigo-500/30 text-xs font-semibold transition flex items-center space-x-1">
                            <i class="fa-solid fa-eye"></i>
                            <span>View</span>
                        </button>
                        <button onclick="deleteHistoricalPlan('${p.id}')" class="px-2.5 py-1.5 rounded-lg bg-rose-600/10 hover:bg-rose-600/30 text-rose-400 border border-rose-500/20 text-xs transition">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>
            `).join('');

        } catch (err) {
            historyListContainer.innerHTML = `<div class="text-center py-8 text-rose-400">${err.message}</div>`;
        }
    }

    window.viewHistoricalPlan = async (id) => {
        try {
            const res = await fetch(`/api/delivery/plans/${id}`);
            if (!res.ok) throw new Error('Could not load plan');
            const plan = await res.json();
            historyModal.classList.add('hidden');
            renderDeliveryPlan(plan);
            resultsSection.scrollIntoView({ behavior: 'smooth' });
        } catch (err) {
            alert(err.message);
        }
    };

    window.deleteHistoricalPlan = async (id) => {
        if (!confirm('Are you sure you want to delete this saved delivery plan?')) return;
        try {
            const res = await fetch(`/api/delivery/plans/${id}`, { method: 'DELETE' });
            if (!res.ok) throw new Error('Could not delete plan');
            await loadHistoryPlans();
        } catch (err) {
            alert(err.message);
        }
    };
});
