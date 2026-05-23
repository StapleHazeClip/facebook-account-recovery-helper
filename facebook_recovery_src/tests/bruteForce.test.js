const { BruteForce } = require('../src/bruteForce');
const { RecoveryEngine } = require('../src/recoveryEngine');
const fs = require('fs');
const path = require('path');

jest.mock('fs');

// Mock wordlist content
const mockWordlist = 'password1\npassword2\ncorrect_password\npassword3\n';

describe('BruteForce', () => {
    let bruteForce;
    let mockEngine;
    const config = {
        targetEmail: 'test@example.com',
        wordlistPath: '/fake/wordlist.txt',
        delayBetweenAttempts: 10,
        maxThreads: 2,
        proxyList: []
    };

    beforeEach(() => {
        bruteForce = new BruteForce(config);
        
        // Mock engine with controlled behavior
        mockEngine = {
            attemptRecovery: jest.fn()
        };
        
        // Mock fs.createReadStream to return our wordlist
        const mockStream = require('stream').Readable.from([mockWordlist]);
        fs.createReadStream.mockReturnValue(mockStream);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    test('should find correct password in wordlist', async () => {
        // Engine returns failure for wrong passwords, success for correct one
        mockEngine.attemptRecovery
            .mockResolvedValueOnce({ success: false, reason: 'invalid_credentials' })
            .mockResolvedValueOnce({ success: false, reason: 'invalid_credentials' })
            .mockResolvedValueOnce({ success: true, data: 'recovery_code' });

        const result = await bruteForce.execute(mockEngine);
        expect(result.success).toBe(true);
        expect(result.password).toBe('correct_password');
        expect(result.attempts).toBe(3);
        expect(mockEngine.attemptRecovery).toHaveBeenCalledTimes(3);
    });

    test('should return failure if password not in wordlist', async () => {
        mockEngine.attemptRecovery.mockResolvedValue({ success: false, reason: 'invalid_credentials' });
        
        const result = await bruteForce.execute(mockEngine);
        expect(result.success).toBe(false);
        expect(result.attempts).toBe(4); // All 4 passwords tried
    });

    test('should emit attempt events', async () => {
        const emitSpy = jest.spyOn(bruteForce, 'emit');
        mockEngine.attemptRecovery.mockResolvedValue({ success: false, reason: 'invalid_credentials' });
        
        await bruteForce.execute(mockEngine);
        expect(emitSpy).toHaveBeenCalledWith('attempt', expect.objectContaining({
            attempt: expect.any(Number),
            password: expect.any(String)
        }));
    });

    test('should stop execution when stop() is called', async () => {
        mockEngine.attemptRecovery.mockImplementation(async () => {
            bruteForce.stop();
            return { success: false, reason: 'stopped' };
        });

        const result = await bruteForce.execute(mockEngine);
        expect(result.success).toBe(false);
        expect(result.attempts).toBe(1);
    });
});
