# Store Cart - Troubleshooting Guide

## Issue: Adding Products to Cart Failing

### Root Causes Addressed in Latest Update

1. **API Route Configuration**
   - **Issue:** CartController was placed in `/Areas/Api/Controllers` which prevented proper route registration
   - **Solution:** Moved CartController to `/Controllers` directory as standard API controller
   - **Location:** `digioz.Portal.Web/Controllers/CartController.cs`

2. **User ID Extraction**
   - **Issue:** User context service didn't exist, causing authentication failures
   - **Solution:** Implemented `GetUserId()` method that checks multiple claim types:
     ```csharp
     private string GetUserId()
     {
         return User.FindFirst("sub")?.Value 
             ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
             ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
     }
     ```

3. **Error Handling & Responses**
   - **Issue:** Generic error messages made debugging difficult
   - **Solution:** API now returns structured JSON responses:
     ```json
     {
       "success": true/false,
       "message": "Detailed error message"
     }
     ```

4. **JavaScript Error Handling**
   - **Issue:** Original script didn't properly handle or display errors
   - **Solution:** Updated all store pages with improved error handling:
     - Better error message display
     - Loading states on buttons
     - Toast notifications for user feedback
     - Graceful error fallbacks

## Verification Steps

### 1. Verify API Endpoint is Accessible

**Test with curl/Postman:**
```
POST /api/cart/add HTTP/1.1
Host: localhost:5000
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "productId": "your-product-id",
  "quantity": 1
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Item added to cart"
}
```

### 2. Check Browser Console

1. Open browser DevTools (F12)
2. Go to Console tab
3. Try adding a product to cart
4. Look for error messages or successful responses
5. Check Network tab to see API request/response details

### 3. Verify User Authentication

- Ensure you're logged in before attempting to add items
- The Add button should be **disabled** if not logged in
- Check that your authentication is configured in `Startup.cs` or `Program.cs`

### 4. Check Database Connectivity

Ensure:
- ShoppingCart table exists in database
- IShoppingCartService is registered in dependency injection
- Database connection string is valid

## Common Error Messages & Solutions

### Error: "Not authenticated"
- **Cause:** User is not logged in or claims are not being extracted properly
- **Solution:** 
  - Log in first
  - Check authentication configuration
  - Verify claims are being populated correctly

### Error: "Product not found"
- **Cause:** ProductId is invalid or product doesn't exist
- **Solution:**
  - Verify the product exists in the database
  - Check that the ProductId is being sent correctly
  - Ensure product is visible (Visible = true)

### Error: "Failed to add item to cart" (generic)
- **Cause:** Database error or unexpected exception
- **Solution:**
  - Check database logs
  - Verify ShoppingCart table structure
  - Check IShoppingCartService implementation

### Button says "Adding..." but nothing happens
- **Cause:** API endpoint not responding
- **Solution:**
  - Check that CartController is in correct location: `/Controllers/CartController.cs`
  - Verify application is restarted after moving controller
  - Check that routing is configured to include API routes

## Debugging Tips

### Enable Detailed Logging

Add to your cart operations:
```csharp
System.Diagnostics.Debug.WriteLine($"Adding product {request.ProductId} for user {userId}");
```

### Check Database Directly

```sql
SELECT * FROM ShoppingCart WHERE UserId = 'your-user-id'
```

### Test Cart Operations Manually

1. Add product via API using Postman
2. Verify in database
3. Check List page updates correctly
4. Test checkout flow

## Configuration Checklist

- [ ] CartController is in `digioz.Portal.Web/Controllers/` directory
- [ ] IShoppingCartService is registered in dependency injection
- [ ] IProductService is registered in dependency injection
- [ ] Authentication is configured and working
- [ ] User is logged in before attempting to add items
- [ ] Browser console shows no errors
- [ ] Network tab shows 200 OK response from API
- [ ] Database ShoppingCart table exists and is accessible

## Files Modified in Latest Update

1. **Created:**
   - `digioz.Portal.Web/Controllers/CartController.cs` - API controller for cart operations

2. **Updated:**
   - `digioz.Portal.Web/Pages/Store/Index.cshtml` - Improved error handling script
   - `digioz.Portal.Web/Pages/Store/ByCategory.cshtml` - Improved error handling script
   - `digioz.Portal.Web/Pages/Store/Details.cshtml` - Improved error handling script

## Next Steps

1. Rebuild the solution
2. Clear browser cache (Ctrl+Shift+Delete)
3. Log in and test adding a product
4. Check browser console for error messages
5. Check Network tab to see API responses
6. Verify items appear in shopping cart (List page)

## Support

If issues persist:
1. Provide browser console error messages
2. Provide Network tab API response details
3. Provide database error logs
4. Provide application error logs
5. Verify all files are in correct locations
