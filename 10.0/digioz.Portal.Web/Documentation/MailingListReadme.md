# Mailing List Management System

## Overview

The Mailing List Management System provides a comprehensive solution for managing email campaigns, subscriber lists, and bulk email distribution. Built on ASP.NET Core 9.0 Razor Pages, it integrates with the existing email provider infrastructure to deliver professional email campaigns.

## Features

### ?? Mailing List Management
- Create and manage multiple mailing lists
- Configure default sender information per list
- Organize subscribers into different lists
- View list details and statistics

### ?? Campaign Management
- Create email campaigns with rich HTML content
- CKEditor integration for professional email design
- Assign campaigns to specific mailing lists
- Preview campaigns before sending
- Track campaign statistics (visitor counts)

### ?? Subscriber Management
- Add and manage email subscribers
- Track subscriber status (Active/Inactive)
- Associate subscribers with multiple mailing lists
- Manage subscriber details (email, name, dates)

### ?? Bulk Email Sending
- Send campaigns to all active subscribers
- Email template system with placeholders
- Success/failure tracking per send operation
- Integration with SendGrid, SMTP, and other email providers

## Architecture

### Database Schema

#### Tables
1. **MailingList**
   - `Id` (PK)
   - `Name`
   - `Address`
   - `DefaultFromName`
   - `DefaultEmailFrom`
   - `Description`

2. **MailingListCampaign**
   - `Id` (PK)
   - `Name`
   - `Subject`
   - `FromName`
   - `FromEmail`
   - `Summary`
   - `Banner`
   - `Body` (HTML content)
   - `VisitorCount`
   - `DateCreated`

3. **MailingListSubscriber**
   - `Id` (PK)
   - `Email`
   - `FirstName`
   - `LastName`
   - `Status` (Active/Inactive)
   - `DateCreated`
   - `DateModified`

4. **MailingListCampaignRelation**
   - `Id` (PK)
   - `MailingListId` (FK)
   - `MailingListCampaignId` (FK)

5. **MailingListSubscriberRelation**
   - `Id` (PK)
   - `MailingListId` (FK)
   - `MailingListSubscriberId` (FK)

### Service Layer

#### Core Services
- `IMailingListService` - Mailing list CRUD operations
- `IMailingListCampaignService` - Campaign CRUD operations
- `IMailingListSubscriberService` - Subscriber CRUD operations
- `IMailingListCampaignRelationService` - Campaign-List associations
- `IMailingListSubscriberRelationService` - Subscriber-List associations

#### Helper Methods
```csharp
// Campaign Relations
List<MailingListCampaignRelation> GetByMailingListId(string mailingListId)
List<MailingListCampaignRelation> GetByCampaignId(string campaignId)
MailingListCampaignRelation GetByMailingListAndCampaign(string mailingListId, string campaignId)
void DeleteByMailingListAndCampaign(string mailingListId, string campaignId)

// Subscriber Relations
List<MailingListSubscriberRelation> GetByMailingListId(string mailingListId)
List<MailingListSubscriberRelation> GetBySubscriberId(string subscriberId)
MailingListSubscriberRelation GetByMailingListAndSubscriber(string mailingListId, string subscriberId)
void DeleteByMailingListAndSubscriber(string mailingListId, string subscriberId)
```

## User Interface

### Navigation
All pages include a shared navigation component (`_MailingListNav.cshtml`) with links to:
- **Mailing Lists** - View and manage lists
- **Campaigns** - Create and send campaigns
- **Subscribers** - Manage subscriber database

### Admin Pages

#### Mailing List Pages (`/Admin/MailingList/`)
- **Index** - List all mailing lists with pagination
  - Actions: View Details, Edit, Delete, Manage Subscribers
- **Add** - Create new mailing list
- **Edit** - Modify mailing list details
- **Delete** - Remove mailing list (with confirmation)
- **Details** - View mailing list information
- **ManageSubscribers** - Two-column interface for adding/removing subscribers

#### Campaign Pages (`/Admin/MailingListCampaign/`)
- **Index** - List all campaigns with pagination
  - Actions: Send, View Details, Edit, Delete
- **Add** - Create new campaign with CKEditor
  - Select mailing list from dropdown
- **Edit** - Modify campaign with CKEditor
  - Update mailing list assignment
- **Delete** - Remove campaign (with confirmation)
- **Details** - View campaign details with rendered HTML
- **SendCampaign** - Send campaign to subscribers
  - Preview email body
  - View subscriber count
  - Confirmation checkbox required
  - Success/failure tracking

#### Subscriber Pages (`/Admin/MailingListSubscriber/`)
- **Index** - List all subscribers with pagination
  - Status badges (Active/Inactive)
- **Add** - Create new subscriber
- **Edit** - Update subscriber information
- **Delete** - Remove subscriber (with confirmation)
- **Details** - View subscriber details

## Email Integration

### Email Template System
Location: `/wwwroot/img/Emails/EmailNotification.htm`

#### Template Placeholders
- `#SITENAME#` - Site name from Config
- `#SITEURL#` - Site URL from Config
- `#TO#` - Personalized greeting with subscriber name
- `#CONTENT#` - Campaign body HTML

### Email Provider Configuration
The system uses the existing `IEmailNotificationService` which supports:
- **SendGrid** - Cloud-based email delivery
- **SMTP** - Traditional SMTP servers
- **Other providers** - Extensible provider system

Configuration is managed through the `Config` table:
- `SiteURL` - Website URL
- `SiteName` - Site display name
- `WebmasterEmail` - Default sender email
- SMTP/SendGrid specific settings

### Email Sending Logic
```csharp
// Priority for From address:
1. Campaign.FromEmail (if set)
2. MailingList.DefaultEmailFrom (if set)
3. Config["WebmasterEmail"] (fallback)
```

## Usage Guide

### Creating a Campaign and Sending Emails

#### 1. Create a Mailing List
```
Admin ? Mailing Lists ? Add Mailing List
- Enter list name
- Set default sender information
- Add description (optional)
- Save
```

#### 2. Add Subscribers to List
```
Admin ? Mailing Lists ? Manage Subscribers (?? icon)
- View available subscribers on left
- Click "Add" to move to subscribed list
- Active subscribers only receive emails
```

#### 3. Create a Campaign
```
Admin ? Campaigns ? Add Campaign
- Enter campaign name and subject
- Select mailing list from dropdown
- Configure sender info (optional - uses list defaults)
- Enter summary text
- Add banner URL (optional)
- Design email body using CKEditor
- Save
```

#### 4. Preview and Send Campaign
```
Admin ? Campaigns ? Send (?? icon)
- Review campaign details
- Preview email body
- Verify subscriber count
- Check confirmation box
- Click "Send Campaign"
- View success/failure counts
```

### Managing Subscribers

#### Add Individual Subscriber
```
Admin ? Subscribers ? Add Subscriber
- Enter email address (required)
- Enter first and last name
- Set status (Active by default)
- Save
```

#### Assign to Mailing List
```
Method 1: From Mailing List
- Navigate to Mailing Lists
- Click "Manage Subscribers" icon
- Add from available list

Method 2: Direct Management
- Manage through MailingListSubscriberRelation table
```

## Email Template Customization

### Modifying the Email Template
Edit: `/wwwroot/img/Emails/EmailNotification.htm`

```html
<!-- Available placeholders -->
#SITENAME# - Your site name
#SITEURL# - Your site URL
#TO# - Personalized greeting
#CONTENT# - Campaign content
```

### Creating Custom Templates
1. Copy `EmailNotification.htm`
2. Modify HTML/CSS as needed
3. Ensure placeholders are preserved
4. Test with sample campaign

## Email Provider Configuration

### SendGrid Setup
Add to `appsettings.json`:
```json
{
  "EmailProvider": {
    "DefaultProvider": "SendGrid",
    "SendGrid": {
      "ApiKey": "YOUR_SENDGRID_API_KEY",
      "EnableClickTracking": true,
      "EnableOpenTracking": true,
      "SandboxMode": false
    }
  }
}
```

### SMTP Setup
Add to `Config` table:
```
SMTPServer = smtp.gmail.com
SMTPPort = 587
SMTPUsername = your@email.com
SMTPPassword = yourpassword
```

## Security Considerations

### Access Control
- All pages require **Administrator** role
- Configured via `[Authorize]` attribute with "AdminOnly" policy
- Area-wide protection in `Program.cs`

### Email Validation
- Email addresses validated on input
- Active status checked before sending
- Duplicate prevention in subscriber relations

### Rate Limiting
Consider implementing:
- Batch sending delays
- Daily send limits
- Provider-specific rate limits

## Troubleshooting

### Common Issues

#### Emails Not Sending
1. Check email provider configuration
2. Verify Config table settings (SiteURL, SiteName, WebmasterEmail)
3. Ensure subscribers are marked as Active
4. Check campaign is assigned to mailing list
5. Review application logs for errors

#### Missing Subscribers in Send
- Verify subscriber Status = true
- Check MailingListSubscriberRelation exists
- Confirm campaign has MailingListCampaignRelation

#### Template Not Loading
- Verify `/wwwroot/img/Emails/EmailNotification.htm` exists
- Check file permissions
- Review web root path configuration

## Database Migrations

### Initial Setup
```sh
# If tables don't exist, they're created automatically via EF Core
# No manual migration needed - context uses EnsureCreated()
```

### Schema Updates
For future changes:
```sh
dotnet ef migrations add MailingListUpdates --context digiozPortalContext
dotnet ef database update --context digiozPortalContext
```

## API Endpoints (Razor Pages)

### Mailing Lists
- `GET /Admin/MailingList/Index` - List all
- `GET /Admin/MailingList/Add` - Add form
- `POST /Admin/MailingList/Add` - Create
- `GET /Admin/MailingList/Edit/{id}` - Edit form
- `POST /Admin/MailingList/Edit/{id}` - Update
- `GET /Admin/MailingList/Delete/{id}` - Delete confirmation
- `POST /Admin/MailingList/Delete/{id}` - Confirm delete
- `GET /Admin/MailingList/Details/{id}` - View details
- `GET /Admin/MailingList/ManageSubscribers/{id}` - Manage UI
- `POST /Admin/MailingList/ManageSubscribers?handler=AddSubscriber` - Add subscriber
- `POST /Admin/MailingList/ManageSubscribers?handler=RemoveSubscriber` - Remove subscriber

### Campaigns
- `GET /Admin/MailingListCampaign/Index` - List all
- `GET /Admin/MailingListCampaign/Add` - Add form
- `POST /Admin/MailingListCampaign/Add` - Create
- `GET /Admin/MailingListCampaign/Edit/{id}` - Edit form
- `POST /Admin/MailingListCampaign/Edit/{id}` - Update
- `GET /Admin/MailingListCampaign/Delete/{id}` - Delete confirmation
- `POST /Admin/MailingListCampaign/Delete/{id}` - Confirm delete
- `GET /Admin/MailingListCampaign/Details/{id}` - View details
- `GET /Admin/MailingListCampaign/SendCampaign/{id}` - Send preview
- `POST /Admin/MailingListCampaign/SendCampaign/{id}` - Execute send

### Subscribers
- `GET /Admin/MailingListSubscriber/Index` - List all
- `GET /Admin/MailingListSubscriber/Add` - Add form
- `POST /Admin/MailingListSubscriber/Add` - Create
- `GET /Admin/MailingListSubscriber/Edit/{id}` - Edit form
- `POST /Admin/MailingListSubscriber/Edit/{id}` - Update
- `GET /Admin/MailingListSubscriber/Delete/{id}` - Delete confirmation
- `POST /Admin/MailingListSubscriber/Delete/{id}` - Confirm delete
- `GET /Admin/MailingListSubscriber/Details/{id}` - View details

## Performance Considerations

### Pagination
All index pages use pagination:
- Default page size: 10 items
- Configurable via query string
- Uses LazZiya.TagHelpers for navigation

### Bulk Sending
When sending to large lists:
- Each email sent individually (no BCC)
- Consider implementing background jobs for 1000+ subscribers
- Monitor email provider rate limits
- Use async/await for non-blocking operations

### Database Queries
- Relations use indexed foreign keys
- GetAll() methods cache-friendly
- Consider implementing caching for large datasets

## Future Enhancements

### Potential Features
- [ ] Campaign scheduling (send at specific time)
- [ ] A/B testing for campaigns
- [ ] Open/click tracking integration
- [ ] Unsubscribe link automation
- [ ] Subscriber import/export (CSV)
- [ ] Campaign templates library
- [ ] Bounce handling
- [ ] List segmentation
- [ ] Analytics dashboard
- [ ] Mobile responsive email templates
- [ ] Background job queue for large sends

## Support

### Resources
- Email Provider Documentation: See `digioz.Portal.EmailProviders` project
- Configuration Guide: Check `Config` table schema
- Template Syntax: Review `EmailNotification.htm`

### Logging
Email operations are logged via `IEmailNotificationService`:
- Success: MessageId returned
- Failure: Error message and exception details
- Check application logs for troubleshooting

## License
Part of the digioz.Portal project. See main repository for license information.

---

**Version**: 1.0  
**Last Updated**: December 2024  
**Framework**: .NET 9.0 / ASP.NET Core Razor Pages
