const { RecoveryEngine } = require('../src/recoveryEngine');
const axios = require('axios');

jest.mock('axios');

// Sample HTML with CSRF token for mocking
const mockInitiateHtml = `
<html>
<body>
<form>
<input name="fb_dtsg" value="mock_csrf_token_12345">
</form>
</body>
</html>
`;

describe('RecoveryEngine', () => {
    let engine;
    const config = {
        targetEmail: 'test@example.com',
        delayBetweenAttempts: 100,
        proxyList: []
    };

    beforeEach(() => {
        engine = new RecoveryEngine(config);
        axios.get.mockResolvedValue({
            data: mockInitiateHtml,
            headers: { 'set-cookie': ['session=abc123; path=/'] }
        });
        axios.post.mockResolvedValue({
            data: '<html>recovery_code</html>',
            status: 200
        });
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    test('should initialize session and extract CSRF token', async () => {
        const session = await engine.initializeSession();
        expect(session.csrfToken).toBe('mock_csrf_token_12345');
        expect(session.cookies).toContain('session=abc123; path=/');
        expect(axios.get).toHaveBeenCalledWith(
            'https://www.facebook.com/recover/initiate',
            expect.any(Object)
        );
    });

    test('should succeed recovery attempt with correct password', async () => {
        const result = await engine.attemptRecovery('test@example.com', 'correct_password');
        expect(result.success).toBe(true);
        expect(result.data).toContain('recovery_code');
    });

    test('should detect checkpoint trigger', async () => {
        axios.post.mockResolvedValueOnce({
            data: '<html>checkpoint</html>',
            status: 200
        });
        const result = await engine.attemptRecovery('test@example.com', 'wrong_password');
        expect(result.success).toBe(false);
        expect(result.reason).toBe('checkpoint_triggered');
    });

    test('should handle network errors gracefully', async () => {
        axios.post.mockRejectedValueOnce(new Error('Network timeout'));
        const result = await engine.attemptRecovery('test@example.com', 'any_password');
        expect(result.success).toBe(false);
        expect(result.reason).toContain('error');
    });
});
