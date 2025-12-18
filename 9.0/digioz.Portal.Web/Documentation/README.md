# DigiOz Portal Web Documentation

Welcome to the DigiOz Portal Web documentation. This folder contains comprehensive guides for developers and administrators working with various features of the portal.

## ?? Documentation Index

### Core Features

#### ?? Shopping Cart & Store
- **[CartTroubleshooting.md](CartTroubleshooting.md)** - Troubleshooting guide for cart functionality
  - Root cause analysis of common cart issues
  - API endpoint verification steps
  - Configuration checklist
  - Debugging tips and error message reference

#### ?? Payment Processing
- **[PaymentProviderConfigurationSteps.md](PaymentProviderConfigurationSteps.md)** - Administrator's step-by-step configuration guide
  - How to configure Authorize.net or PayPal
  - API credential setup
  - Setting the active payment provider
  - Verification and testing steps

- **[PaymentProviderImplementationSummary.md](PaymentProviderImplementationSummary.md)** - Complete technical implementation details
  - Architecture and design decisions
  - How payment provider selection works
  - Code changes and integration details
  - Error handling and security considerations

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

---

## ?? How to Use This Documentation

### For Administrators
1. **Payment Setup**: Start with [PaymentProviderConfigurationSteps.md](PaymentProviderConfigurationSteps.md)
2. **Email Configuration**: See [EmailProviderReadMe.md](EmailProviderReadMe.md) for provider setup
3. **Mailing Lists**: Use [MailingListReadme.md](MailingListReadme.md) to manage campaigns
4. **Troubleshooting**: Check [CartTroubleshooting.md](CartTroubleshooting.md) for common issues

### For Developers
1. **Payment Integration**: Read [PaymentProviderImplementationSummary.md](PaymentProviderImplementationSummary.md)
2. **Email Services**: Review [EmailProviderReadMe.md](EmailProviderReadMe.md) and [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md)
3. **Advanced Features**: Explore [LinkCheckerReadMe.md](LinkCheckerReadMe.md) for architecture details
4. **Troubleshooting**: Use [CartTroubleshooting.md](CartTroubleshooting.md) for debugging

### For New Team Members
Start here:
1. [README.md](README.md) (this file) - Overview
2. [CartTroubleshooting.md](CartTroubleshooting.md) - Understanding core systems
3. [EmailProviderReadMe.md](EmailProviderReadMe.md) - Email system basics
4. [PaymentProviderImplementationSummary.md](PaymentProviderImplementationSummary.md) - Payment architecture

---

## ?? Quick Links by Task

### "I need to configure payments"
? [PaymentProviderConfigurationSteps.md](PaymentProviderConfigurationSteps.md)

### "Payment processing isn't working"
? [PaymentProviderImplementationSummary.md](PaymentProviderImplementationSummary.md) (Troubleshooting section)

### "I need to set up email sending"
? [EmailProviderReadMe.md](EmailProviderReadMe.md)

### "Forgot password emails aren't sending"
? [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md) (Troubleshooting section)

### "Shopping cart has issues"
? [CartTroubleshooting.md](CartTroubleshooting.md)

### "I need to manage email campaigns"
? [MailingListReadme.md](MailingListReadme.md)

### "I want to validate external links"
? [LinkCheckerReadMe.md](LinkCheckerReadMe.md)

---

## ?? Security Considerations

All documentation includes security best practices:

### Email Configuration
- Sensitive credentials are encrypted at rest
- API keys and passwords should be stored in secure configuration
- Use Azure Key Vault for production environments

### Payment Processing
- All card data is handled by secure payment gateways
- No sensitive data is logged
- HTTPS is required for all payment communication

### Link Validation
- SSRF (Server-Side Request Forgery) protection prevents attacks
- Private IP ranges and localhost are blocked
- Cloud metadata services (AWS, Azure, GCP) are blocked

---

## ?? Document Status

| Document | Updated | Status | Audience |
|----------|---------|--------|----------|
| CartTroubleshooting.md | Dec 17, 2025 | ? Current | Developers |
| EmailProviderReadMe.md | Dec 3, 2025 | ? Current | Developers, Admins |
| ForgotPasswordReadMe.md | Dec 3, 2025 | ? Current | Developers, Admins |
| LinkCheckerReadMe.md | Dec 6, 2025 | ? Current | Developers |
| MailingListReadme.md | Dec 4, 2025 | ? Current | Developers, Admins |
| PaymentProviderConfigurationSteps.md | Dec 17, 2025 | ? Current | Administrators |
| PaymentProviderImplementationSummary.md | Dec 17, 2025 | ? Current | Developers |

---

## ?? Related Projects

These documents reference related projects in the solution:

- **digioz.Portal.EmailProviders** - Email provider library
- **digioz.Portal.PaymentProviders** - Payment provider library
- **digioz.Portal.Dal** - Data access layer with services
- **digioz.Portal.Bo** - Business objects and view models
- **digioz.Portal.Utilities** - Shared utilities (encryption, etc.)

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
- ? **Tip** - Best practice or helpful hint
- ?? **Debug** - Troubleshooting guidance

### Status Indicators
- ? **Current** - Up-to-date and relevant
- ?? **Review Needed** - May need updates
- ? **Outdated** - Should be updated or removed

---

## ?? Getting Started

### New to the DigiOz Portal?
1. Read this README
2. Review [CartTroubleshooting.md](CartTroubleshooting.md) for core concepts
3. Choose your area of interest from the documentation index
4. Explore related project documentation

### Setting up a Development Environment?
1. Configure payment providers: [PaymentProviderConfigurationSteps.md](PaymentProviderConfigurationSteps.md)
2. Set up email service: [EmailProviderReadMe.md](EmailProviderReadMe.md)
3. Test forgot password: [ForgotPasswordReadMe.md](ForgotPasswordReadMe.md)
4. Verify shopping cart: [CartTroubleshooting.md](CartTroubleshooting.md)

---

## ?? Additional Resources

### Documentation by Feature
- **Shopping**: Cart, Products, Store Pages
- **Payments**: Authorization, Transactions, Orders
- **Communication**: Email, Notifications, Mailing Lists
- **Utilities**: Link Validation, Configuration Management

### Related Documentation
- **API Documentation**: See API project docs
- **Database Schema**: Check migration files in Dal project
- **Deployment Guide**: See main repository documentation
- **Architecture**: Review related projects' README files

---

**Last Updated**: December 17, 2025  
**Documentation Version**: 1.0  
**Framework**: .NET 9.0 / ASP.NET Core Razor Pages

