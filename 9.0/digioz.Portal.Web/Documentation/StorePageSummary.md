# E-Commerce Store System - Pages Summary

## Overview
A complete user-facing e-commerce store system with product browsing, shopping cart, order history, and checkout functionality.

## Store Pages Created

### 1. `/Store/Index` - Main Store Page
**Purpose:** Display all products with pagination  
**Features:**
- Paginated product grid (12 products per page)
- Product image thumbnails with fallback placeholder
- Product name, short description, and price display
- Add to cart button (only for logged-in users)
- View/Details button for each product
- Out of stock indicators
- LazZiya pagination control

**Files:**
- `digioz.Portal.Web/Pages/Store/Index.cshtml`
- `digioz.Portal.Web/Pages/Store/Index.cshtml.cs`

---

### 2. `/Store/ByCategory/{id}` - Products by Category
**Purpose:** Display products filtered by category with pagination  
**Features:**
- Category name and description header
- Filtered product listing (12 per page)
- Same product display as Index page
- Back to all products link
- 404 handling for invalid categories

**Parameters:**
- `id` - Product Category ID (GUID)
- `pageNumber` - Pagination control

**Files:**
- `digioz.Portal.Web/Pages/Store/ByCategory.cshtml`
- `digioz.Portal.Web/Pages/Store/ByCategory.cshtml.cs`

---

### 3. `/Store/Details/{id}` - Product Details
**Purpose:** Show complete product information  
**Features:**
- Full-size product image
- Complete product details (SKU, Make, Model, Weight, Dimensions, etc.)
- Category information
- Product description (rendered as HTML)
- Product options/variations table
- Manufacturing information
- Add to cart button
- View counter increment
- Status indicators (out of stock, etc.)

**Parameters:**
- `id` - Product ID (GUID)

**Files:**
- `digioz.Portal.Web/Pages/Store/Details.cshtml`
- `digioz.Portal.Web/Pages/Store/Details.cshtml.cs`

---

### 4. `/Store/List` - Shopping Cart
**Purpose:** Display and manage shopping cart items  
**Features:**
- Requires authentication (auto-redirects if not logged in)
- Editable quantity for each item
- Calculate subtotals and total
- Remove individual items
- Empty entire cart button
- Update quantities via API
- Back to shopping link
- Checkout button

**Files:**
- `digioz.Portal.Web/Pages/Store/List.cshtml`
- `digioz.Portal.Web/Pages/Store/List.cshtml.cs`

---

### 5. `/Store/History` - Order History
**Purpose:** Show user's past orders and transactions  
**Features:**
- Requires authentication
- Paginated order listing (10 per page)
- Order details including:
  - Order number and date
  - Shipping and billing addresses
  - Order items with quantities and prices
  - Payment status
  - Order total
- Order details table showing products and amounts
- Payment status badges

**Files:**
- `digioz.Portal.Web/Pages/Store/History.cshtml`
- `digioz.Portal.Web/Pages/Store/History.cshtml.cs`

---

### 6. `/Store/Checkout` - Checkout & Payment
**Purpose:** Collect customer information and process payment  
**Features:**
- Requires authentication
- Order summary with items and total
- Billing information form:
  - First/Last Name
  - Billing Address (2 lines)
  - City, State/Province, ZIP/Postal Code, Country
- Shipping information form:
  - Toggle to use same as billing
  - Optional different shipping address
- Payment information form:
  - Card number
  - Expiration date
  - CVV/Security code
- Payment provider selection (configurable)
- Order generation and cart clearing upon success
- Redirect to order confirmation

**Payment Providers Supported:**
- Stripe (placeholder implementation)
- PayPal (placeholder implementation)
- Square (placeholder implementation)

**Configuration Required:**
- `PaymentProviderType` - Which provider to use
- Provider-specific API keys (see STORE_CONFIG_KEYS.md)

**Files:**
- `digioz.Portal.Web/Pages/Store/Checkout.cshtml`
- `digioz.Portal.Web/Pages/Store/Checkout.cshtml.cs`

---

### 7. `/Store/OrderConfirmation/{orderId}` - Order Confirmation
**Purpose:** Display successful order confirmation  
**Features:**
- Order number and date
- Order total
- Shipping address
- Order line items with quantities and prices
- Payment status
- Confirmation message
- Links to continue shopping or view all orders
- Email confirmation notification

**Parameters:**
- `orderId` - Order ID (GUID)

**Files:**
- `digioz.Portal.Web/Pages/Store/OrderConfirmation.cshtml`
- `digioz.Portal.Web/Pages/Store/OrderConfirmation.cshtml.cs`

---

## API Endpoints

### Cart API Controller (`/api/cart`)
Located at: `digioz.Portal.Web/Controllers/CartController.cs`

#### `POST /api/cart/add`
Add product to shopping cart
```json
{
  "productId": "guid",
  "quantity": 1
}
```

#### `DELETE /api/cart/remove/{cartId}`
Remove item from cart

#### `PUT /api/cart/update`
Update cart item quantity
```json
{
  "id": "cartId",
  "quantity": 5
}
```

#### `DELETE /api/cart/empty`
Empty entire shopping cart for current user

---

## Key Features

### Authentication & Authorization
- All store pages except Index and ByCategory check for authentication
- Add to cart disabled for anonymous users (button shows disabled state)
- Cart, History, and Checkout require `[Authorize]` attribute
- User ID extracted from claims-based authentication

### Image Handling
- Thumbnail images (200x200) stored in `/wwwroot/img/Products/Thumb/`
- Full-size images stored in `/wwwroot/img/Products/Full/`
- Fallback placeholder icon for products without images
- Responsive image sizing

### Pagination
- Uses LazZiya.TagHelpers paging control
- Configurable page sizes per page
- Default: 12 products per page for Store, 10 orders per page for History

### Error Handling
- User-friendly error messages
- API endpoints return JSON with success/failure indicators
- Try-catch blocks with detailed error logging
- Graceful degradation for missing data

### User Experience
- Real-time cart updates via AJAX
- Loading states and animations
- Toast notifications for user feedback
- Modal confirmations for destructive actions
- Responsive design using Bootstrap 5

---

## Database Tables Used

- **Product** - Product catalog
- **ProductCategory** - Product categories
- **ProductOption** - Product variations/options
- **ShoppingCart** - User's cart items
- **Order** - Customer orders
- **OrderDetail** - Order line items
- **Config** - Configuration settings

---

## Configuration Requirements

See `STORE_CONFIG_KEYS.md` for complete list of required configuration keys:
- Payment provider selection
- Payment provider API credentials
- Store name and currency
- Email configuration

---

## Security Notes

1. All payment endpoints require authentication
2. Cart operations verify user ownership of items
3. Sensitive configuration stored in Config table (should use Key Vault in production)
4. CSRF protection enabled on all forms
5. Authorization checks on all protected endpoints

---

## Future Enhancements

1. Implement actual payment provider integrations (Stripe, PayPal, Square)
2. Add discount codes/coupon system
3. Implement shipping calculation
4. Add tax calculation
5. Email notification system for orders
6. Wishlist/favorites feature
7. Product reviews and ratings
8. Inventory management
9. Customer account pages (profile, address book)
10. Admin order management interface
