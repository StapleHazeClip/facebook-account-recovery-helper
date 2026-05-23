/**
 * Simple logger utility for the recovery helper.
 * Provides colored output for different log levels.
 */
class Logger {
    constructor() {
        this.colors = {
            info: '\x1b[36m',    // Cyan
            success: '\x1b[32m', // Green
            warning: '\x1b[33m', // Yellow
            error: '\x1b[31m',   // Red
            reset: '\x1b[0m'     // Reset
        };
    }

    _timestamp() {
        return new Date().toISOString().replace('T', ' ').substring(0, 19);
    }

    info(message) {
        console.log(`${this.colors.info}[${this._timestamp()}] [INFO] ${message}${this.colors.reset}`);
    }

    success(message) {
        console.log(`${this.colors.success}[${this._timestamp()}] [SUCCESS] ${message}${this.colors.reset}`);
    }

    warning(message) {
        console.log(`${this.colors.warning}[${this._timestamp()}] [WARNING] ${message}${this.colors.reset}`);
    }

    error(message) {
        console.error(`${this.colors.error}[${this._timestamp()}] [ERROR] ${message}${this.colors.reset}`);
    }
}

module.exports = new Logger();
