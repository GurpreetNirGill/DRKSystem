// Dashboard JavaScript Functions

window.showEmergencyAlert = (message) => {
    // Create emergency alert overlay
    const overlay = document.createElement('div');
    overlay.className = 'emergency-alert-overlay';
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(220, 53, 69, 0.95);
        z-index: 9999;
        display: flex;
        align-items: center;
        justify-content: center;
        animation: emergencyFlash 1s ease-in-out infinite alternate;
    `;

    const alertBox = document.createElement('div');
    alertBox.className = 'emergency-alert-box';
    alertBox.style.cssText = `
        background: white;
        padding: 2rem;
        border-radius: 8px;
        text-align: center;
        max-width: 500px;
        border: 3px solid #dc3545;
        box-shadow: 0 10px 30px rgba(0,0,0,0.3);
    `;

    alertBox.innerHTML = `
        <h2 style="color: #dc3545; margin-bottom: 1rem;">
            🚨 NOTFALL GEMELDET
        </h2>
        <p style="font-size: 1.2rem; margin-bottom: 1.5rem;">
            ${message}
        </p>
        <button onclick="closeEmergencyAlert()" 
                style="background: #dc3545; color: white; border: none; padding: 0.75rem 2rem; border-radius: 4px; font-size: 1rem; cursor: pointer;">
            VERSTANDEN
        </button>
    `;

    overlay.appendChild(alertBox);
    document.body.appendChild(overlay);

    // Auto-close after 10 seconds
    setTimeout(() => {
        closeEmergencyAlert();
    }, 10000);

    // Play emergency sound
    playEmergencySound();
};

window.closeEmergencyAlert = () => {
    const overlay = document.querySelector('.emergency-alert-overlay');
    if (overlay) {
        overlay.remove();
    }
};

window.playEmergencySound = () => {
    try {
        // Create audio context for emergency sound
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        
        // Generate emergency beep sound
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();
        
        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);
        
        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
        
        oscillator.start();
        oscillator.stop(audioContext.currentTime + 0.5);
        
        // Repeat beep 3 times
        setTimeout(() => {
            const osc2 = audioContext.createOscillator();
            const gain2 = audioContext.createGain();
            osc2.connect(gain2);
            gain2.connect(audioContext.destination);
            osc2.frequency.setValueAtTime(800, audioContext.currentTime);
            gain2.gain.setValueAtTime(0.3, audioContext.currentTime);
            osc2.start();
            osc2.stop(audioContext.currentTime + 0.5);
        }, 700);
        
        setTimeout(() => {
            const osc3 = audioContext.createOscillator();
            const gain3 = audioContext.createGain();
            osc3.connect(gain3);
            gain3.connect(audioContext.destination);
            osc3.frequency.setValueAtTime(800, audioContext.currentTime);
            gain3.gain.setValueAtTime(0.3, audioContext.currentTime);
            osc3.start();
            osc3.stop(audioContext.currentTime + 0.5);
        }, 1400);
        
    } catch (error) {
        console.warn('Emergency sound failed:', error);
    }
};

// Toast Notifications
window.showToast = (message, type = 'info') => {
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${getToastColor(type)};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 4px;
        z-index: 1000;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        animation: slideInRight 0.3s ease-out;
    `;
    
    toast.textContent = message;
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.3s ease-in';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
};

function getToastColor(type) {
    switch (type) {
        case 'success': return '#28a745';
        case 'warning': return '#ffc107';
        case 'error': return '#dc3545';
        case 'info': 
        default: return '#17a2b8';
    }
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes emergencyFlash {
        0% { background: rgba(220, 53, 69, 0.9); }
        100% { background: rgba(220, 53, 69, 1); }
    }
    
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
`;
document.head.appendChild(style);

// Session Timer Updates
window.updateSessionTimer = (sessionId, duration) => {
    const timer = document.querySelector(`[data-session-id="${sessionId}"] .session-timer`);
    if (timer) {
        const minutes = Math.floor(duration / 60);
        const seconds = duration % 60;
        timer.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        // Warning colors
        if (duration > 25 * 60) { // > 25 minutes
            timer.className = 'session-timer text-danger fw-bold';
        } else if (duration > 20 * 60) { // > 20 minutes
            timer.className = 'session-timer text-warning fw-bold';
        } else {
            timer.className = 'session-timer text-primary';
        }
    }
};

// Real-time capacity updates
window.updateCapacityIndicator = (activeSessions, totalCapacity) => {
    const indicator = document.querySelector('.capacity-indicator');
    if (indicator) {
        const percentage = (activeSessions / totalCapacity) * 100;
        indicator.style.width = `${percentage}%`;
        
        if (percentage >= 100) {
            indicator.className = 'capacity-indicator bg-danger';
        } else if (percentage >= 80) {
            indicator.className = 'capacity-indicator bg-warning';
        } else {
            indicator.className = 'capacity-indicator bg-success';
        }
    }
};

console.log('🏥 DKR Dashboard JavaScript loaded successfully!');