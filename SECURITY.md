# Security Policy

## Supported Versions

We provide security updates for the following versions of qf4net:

| Version | Supported          |
| ------- | ------------------ |
| 25.9.x  | :white_check_mark: |
| < 25.9  | :x:                |

## Reporting a Vulnerability

The qf4net team takes security bugs seriously. We appreciate your efforts to responsibly disclose your findings, and will make every effort to acknowledge your contributions.

### How to Report

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities by emailing: **[security@qf4net.org]** (or create a private security advisory on GitHub)

Include the following information in your report:
- Type of issue (buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit it

### What to Expect

After reporting a vulnerability, you can expect:

1. **Acknowledgment**: We'll acknowledge receipt of your vulnerability report within 2 business days
2. **Assessment**: We'll assess the vulnerability and determine its severity and impact within 5 business days
3. **Updates**: We'll keep you informed about our progress at least every 7 days
4. **Resolution**: We'll work to resolve the issue as quickly as possible based on its severity:
   - **Critical/High**: Within 7 days
   - **Medium**: Within 30 days
   - **Low**: Within 90 days

### Disclosure Policy

We follow coordinated disclosure principles:
- We ask that you give us a reasonable amount of time to investigate and fix the issue before public disclosure
- We will publicly acknowledge your responsible disclosure after the fix is released (unless you prefer to remain anonymous)
- If you've followed this process, we will not pursue legal action against you regarding the report

### Security Best Practices

When using qf4net in production:

1. **Keep Updated**: Always use the latest supported version
2. **Input Validation**: Validate all input data before processing
3. **Thread Safety**: Be aware of thread safety implications when using qf4net in multi-threaded environments
4. **Error Handling**: Implement proper error handling and logging
5. **Monitoring**: Monitor your applications for unusual behavior

### Scope

This security policy applies to:
- The main qf4net library (`src/qf4net/`)
- Official examples and documentation
- CI/CD infrastructure

Out of scope:
- Third-party dependencies (report to their respective maintainers)
- General software engineering questions
- Performance issues (unless they could lead to DoS)

### Recognition

We believe in recognizing security researchers who help make qf4net safer. With your permission, we will:
- Credit you in our security advisories
- Add you to our security researchers acknowledgment list
- Provide you with advance notice of security-related releases

### Contact

For security-related questions or concerns:
- Security email: [security@qf4net.org]
- General issues: [GitHub Issues](https://github.com/zdomokos/qf4net/issues)
- Discussions: [GitHub Discussions](https://github.com/zdomokos/qf4net/discussions)

Thank you for helping to keep qf4net secure!