# DigiOz Portal Web Documentation

Welcome to the DigiOz Portal Web documentation. This folder contains comprehensive guides for developers and administrators working with various features of the portal.

## ?? Documentation Index

### Core Features

#### ??? Security & Rate Limiting
- **[RateLimiting_Guide.md](RateLimiting_Guide.md)** - **? COMPREHENSIVE GUIDE** for rate limiting and bot protection
  - Complete architecture overview
  - Database schema and configuration
  - Admin dashboard usage
  - Testing and troubleshooting
  - API reference and monitoring
  - Performance optimization
  - Security best practices

#### ?? Shopping Cart & Store
- **[CartTroubleshooting.md](CartTroubleshooting.md)** - Troubleshooting guide for cart functionality
  - Root cause analysis of common cart issues
  - API endpoint verification steps
  - Configuration checklist
  - Debugging tips and error message reference

#### ?? Email & Notifications
- **[EmailProviderReadMe.md](EmailProviderReadMe.md)** - Complete email provider documentation
  - Supported providers: SMTP, SendGrid, Mailgun, Azure Communication Services
  - Configuration for each provider
  - Usage examples and code samples
  - Error handling, testing, and troubleshooting

- **[ForgotPasswordReadMe.md](ForgotPasswordReadMe.md)** - Forgot password feature documentation
  - Implementation details with encryption support
  - Configuration requirements
  - Email template information
  - Security features and best practices
  - Testing and troubleshooting

#### ?? Mailing List Management
- **[MailingListReadme.md](MailingListReadme.md)** - Mailing list system documentation
  - Feature overview (lists, campaigns, subscribers)
  - Database schema and service layer
  - Admin UI pages and workflows
  - Email integration and provider configuration
  - Usage guide for administrators and developers

#### ?? Link Management
- **[LinkCheckerReadMe.md](LinkCheckerReadMe.md)** - Link validation service documentation
  - Architecture and components
  - HTTP checking strategy with fallback
  - Automatic metadata extraction
  - SSRF protection and security measures
  - Performance optimizations and resource management
  - Thread safety and cancellation support

#### ?? Infrastructure
- **[Cloudflare_IP_Detection.md](Cloudflare_IP_Detection.md)** - IP detection behind proxies
  - Proper IP address detection for Cloudflare and other proxies
  - Security considerations
  - Configuration for rate limiting

---

## ?? How to Use This Documentation

### For Administrators
1. **Security Setup**: Start with [RateLimiting_Guide.md](RateLimiting_Guide.md) for protection
2. **Email Configuration**: See [EmailProviderReadMe.md](EmailProviderReadMe.md) for provider setup
3. **Mailing Lists**: Use [MailingListReadme.md](MailingListReadme.md) to manage campaigns
4. **Troubleshooting**: Check [CartTroubleshooting.md](CartTroubleshooting.md) for common issues

### For Developers
1. **Rate Limiting**: Read [RateLimiting_Guide.md](RateLimiting_Guide.md) for complete architecture
2. **Email Services**: Review [EmailProviderReadMe.md](EmailProviderReadMe.md) and [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md)
3. **Advanced Features**: Explore [LinkCheckerReadMe.md](LinkCheckerReadMe.md) for architecture details
4. **Proxy Configuration**: See [Cloudflare_IP_Detection.md](Cloudflare_IP_Detection.md)

### For New Team Members
Start here:
1. [README.md](README.md) (this file) - Overview
2. [RateLimiting_Guide.md](RateLimiting_Guide.md) - Security fundamentals
3. [CartTroubleshooting.md](CartTroubleshooting.md) - Understanding core systems
4. [EmailProviderReadMe.md](EmailProviderReadMe.md) - Email system basics

---

## ?? Quick Links by Task

### "I need to protect my site from bots/attacks"
?? [RateLimiting_Guide.md](RateLimiting_Guide.md)

### "I need to configure rate limiting"
?? [RateLimiting_Guide.md](RateLimiting_Guide.md) - Configuration section

### "An IP is banned and shouldn't be"
?? [RateLimiting_Guide.md](RateLimiting_Guide.md) - Admin Dashboard & Troubleshooting

### "I'm behind Cloudflare and rate limiting isn't working"
?? [Cloudflare_IP_Detection.md](Cloudflare_IP_Detection.md)

### "I need to set up email sending"
?? [EmailProviderReadMe.md](EmailProviderReadMe.md)

### "Forgot password emails aren't sending"
?? [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md) (Troubleshooting section)

### "Shopping cart has issues"
?? [CartTroubleshooting.md](CartTroubleshooting.md)

### "I need to manage email campaigns"
?? [MailingListReadme.md](MailingListReadme.md)

### "I want to validate external links"
?? [LinkCheckerReadMe.md](LinkCheckerReadMe.md)

---

## ?? Security Considerations

All documentation includes security best practices:

### Rate Limiting & Bot Protection
- Protects against DDoS, brute force, and scraping attacks
- Automatic IP banning with escalating penalties
- Password reset enumeration prevention
- Admin dashboard for ban management
- See: [RateLimiting_Guide.md](RateLimiting_Guide.md)

### Email Configuration
- Sensitive credentials are encrypted at rest
- API keys and passwords should be stored in secure configuration
- Use Azure Key Vault for production environments

### Link Validation
- SSRF (Server-Side Request Forgery) protection prevents attacks
- Private IP ranges and localhost are blocked
- Cloud metadata services (AWS, Azure, GCP) are blocked

---

## ?? Document Status

| Document | Updated | Status | Audience |
|----------|---------|--------|----------|
| RateLimiting_Guide.md | Jan 29, 2026 | ? Current | Developers, Admins |
| Cloudflare_IP_Detection.md | Jan 2026 | ? Current | Developers, Admins |
| CartTroubleshooting.md | Dec 17, 2025 | ? Current | Developers |
| EmailProviderReadMe.md | Dec 3, 2025 | ? Current | Developers, Admins |
| ForgotPasswordReadMe.md | Dec 3, 2025 | ? Current | Developers, Admins |
| LinkCheckerReadMe.md | Dec 6, 2025 | ? Current | Developers |
| MailingListReadme.md | Dec 4, 2025 | ? Current | Developers, Admins |

---

## ?? Related Projects

These documents reference related projects in the solution:

- **digioz.Portal.Dal** - Data access layer with services (includes rate limiting cleanup)
- **digioz.Portal.Bo** - Business objects (includes BannedIp, BannedIpTracking)
- **digioz.Portal.Utilities** - Shared utilities (encryption, IP detection, bot detection)
- **digioz.Portal.EmailProviders** - Email provider library
- **digioz.Portal.PaymentProviders** - Payment provider library

---

## ?? Support & Contributions

### Reporting Issues
If you find outdated information or have suggestions:
1. Check the document status table above
2. Review the GitHub repository issues
3. Contact the development team

### Contributing Documentation
To contribute updates:
1. Ensure accuracy of technical content
2. Include code examples where applicable
3. Add troubleshooting sections for complex features
4. Update the README index with new files
5. Include version and date information

---

## ?? Document Conventions

### Code Examples
Code examples use language identifiers:
```csharp
// C# examples
public class MyClass { }
```

```json
// JSON configuration
{
  "key": "value"
}
```

```sql
-- SQL queries
SELECT * FROM Config
```

### Important Sections
- ?? **Warning** - Important security or data considerations
- ?? **Note** - Additional helpful information
- ?? **Tip** - Best practice or helpful hint
- ?? **Debug** - Troubleshooting guidance

### Status Indicators
- ? **Current** - Up-to-date and relevant
- ?? **Review Needed** - May need updates
- ? **Outdated** - Should be updated or removed

---

## ?? Getting Started

### New to the DigiOz Portal?
1. Read this README
2. Review [RateLimiting_Guide.md](RateLimiting_Guide.md) for security fundamentals
3. Review [CartTroubleshooting.md](CartTroubleshooting.md) for core concepts
4. Choose your area of interest from the documentation index
5. Explore related project documentation

### Setting up a Development Environment?
1. **Enable security**: [RateLimiting_Guide.md](RateLimiting_Guide.md)
2. Set up email service: [EmailProviderReadMe.md](EmailProviderReadMe.md)
3. Test forgot password: [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md)
4. Verify shopping cart: [CartTroubleshooting.md](CartTroubleshooting.md)
5. Configure proxy detection: [Cloudflare_IP_Detection.md](Cloudflare_IP_Detection.md)

---

## ?? Additional Resources

### Documentation by Feature
- **Security**: Rate Limiting, Bot Protection, IP Banning
- **Shopping**: Cart, Products, Store Pages
- **Communication**: Email, Notifications, Mailing Lists
- **Utilities**: Link Validation, Configuration Management
- **Infrastructure**: IP Detection, Proxy Configuration

### Related Documentation
- **API Documentation**: See API project docs
- **Database Schema**: Check migration files in Dal project
- **Deployment Guide**: See main repository documentation
- **Architecture**: Review related projects' README files

---

**Last Updated**: January 29, 2026  
**Documentation Version**: 2.0  
**Framework**: .NET 9.0 / ASP.NET Core Razor Pages

