using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Dal;
using digioz.Portal.Bll;

namespace digioz.Portal.Web
{
    public static class DependencyConfig
    {
        public static void AddDependencies(ref IServiceCollection services, IConfiguration configuration) 
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton<IConfigHelper >( new ConfigHelper(connectionString));

            services.AddSingleton<IRepo<Announcement>, BaseRepo<Announcement>>();
            services.AddSingleton<IRepo<AspNetUser>, BaseRepo<AspNetUser>>();
            services.AddSingleton<IRepo<AspNetRole>, BaseRepo<AspNetRole>>();
            services.AddSingleton<IRepo<AspNetUserRole>, BaseRepo<AspNetUserRole>>();
            services.AddSingleton<IRepo<Chat>, BaseRepo<Chat>>();
            services.AddSingleton<IRepo<CommentConfig>, BaseRepo<CommentConfig>>();
            services.AddSingleton<IRepo<CommentLike>, BaseRepo<CommentLike>>();
            services.AddSingleton<IRepo<Comment>, BaseRepo<Comment>>();
            services.AddSingleton<IRepo<Config>, BaseRepo<Config>>();
            services.AddSingleton<IRepo<LinkCategory>, BaseRepo<LinkCategory>>();
            services.AddSingleton<IRepo<Link>, BaseRepo<Link>>();
            services.AddSingleton<IRepo<Log>, BaseRepo<Log>>();
            services.AddSingleton<IRepo<LogVisitor>, BaseRepo<LogVisitor>>();
            services.AddSingleton<IRepo<MailingListCampaign>, BaseRepo<MailingListCampaign>>();
            services.AddSingleton<IRepo<MailingListCampaignRelation>, BaseRepo<MailingListCampaignRelation>>();
            services.AddSingleton<IRepo<MailingList>, BaseRepo<MailingList>>();
            services.AddSingleton<IRepo<MailingListSubscriber>, BaseRepo<MailingListSubscriber>>();
            services.AddSingleton<IRepo<MailingListSubscriberRelation>, BaseRepo<MailingListSubscriberRelation>>();
            services.AddSingleton<IRepo<Menu>, BaseRepo<Menu>>();
            services.AddSingleton<IRepo<Module>, BaseRepo<Module>>();
            services.AddSingleton<IRepo<OrderDetail>, BaseRepo<OrderDetail>>();
            services.AddSingleton<IRepo<Order>, BaseRepo<Order>>();
            services.AddSingleton<IRepo<Page>, BaseRepo<Page>>();
            services.AddSingleton<IRepo<PictureAlbum>, BaseRepo<PictureAlbum>>();
            services.AddSingleton<IRepo<Picture>, BaseRepo<Picture>>();
            services.AddSingleton<IRepo<Plugin>, BaseRepo<Plugin>>();
            services.AddSingleton<IRepo<PollAnswer>, BaseRepo<PollAnswer>>();
            services.AddSingleton<IRepo<Poll>, BaseRepo<Poll>>();
            services.AddSingleton<IRepo<PollUsersVote>, BaseRepo<PollUsersVote>>();
            services.AddSingleton<IRepo<PollVote>, BaseRepo<PollVote>>();
            services.AddSingleton<IRepo<ProductCategory>, BaseRepo<ProductCategory>>();
            services.AddSingleton<IRepo<Product>, BaseRepo<Product>>();
            services.AddSingleton<IRepo<ProductOption>, BaseRepo<ProductOption>>();
            services.AddSingleton<IRepo<Profile>, BaseRepo<Profile>>();
            services.AddSingleton<IRepo<Rss>, BaseRepo<Rss>>();
            services.AddSingleton<IRepo<ShoppingCart>, BaseRepo<ShoppingCart>>();
            services.AddSingleton<IRepo<SlideShow>, BaseRepo<SlideShow>>();
            services.AddSingleton<IRepo<VideoAlbum>, BaseRepo<VideoAlbum>>();
            services.AddSingleton<IRepo<Video>, BaseRepo<Video>>();
            services.AddSingleton<IRepo<VisitorInfo>, BaseRepo<VisitorInfo>>();
            services.AddSingleton<IRepo<VisitorSession>, BaseRepo<VisitorSession>>();
            services.AddSingleton<IRepo<Zone>, BaseRepo<Zone>>();

            services.AddSingleton<IChatRepo, ChatRepo>();
            services.AddSingleton<ICommentRepo, CommentRepo>();

            services.AddSingleton<ILogic<Announcement>, BaseLogic<Announcement>>();
            services.AddSingleton<ILogic<AspNetUser>, BaseLogic<AspNetUser>>();
            services.AddSingleton<ILogic<AspNetRole>, BaseLogic<AspNetRole>>();
            services.AddSingleton<ILogic<AspNetUserRole>, BaseLogic<AspNetUserRole>>();
            services.AddSingleton<ILogic<Chat>, BaseLogic<Chat>>();
            services.AddSingleton<ILogic<CommentConfig>, BaseLogic<CommentConfig>>();
            services.AddSingleton<ILogic<CommentLike>, BaseLogic<CommentLike>>();
            services.AddSingleton<ILogic<Comment>, BaseLogic<Comment>>();
            services.AddSingleton<ILogic<Config>, BaseLogic<Config>>();
            services.AddSingleton<ILogic<LinkCategory>, BaseLogic<LinkCategory>>();
            services.AddSingleton<ILogic<Link>, BaseLogic<Link>>();
            services.AddSingleton<ILogic<Log>, BaseLogic<Log>>();
            services.AddSingleton<ILogic<LogVisitor>, BaseLogic<LogVisitor>>();
            services.AddSingleton<ILogic<MailingListCampaign>, BaseLogic<MailingListCampaign>>();
            services.AddSingleton<ILogic<MailingListCampaignRelation>, BaseLogic<MailingListCampaignRelation>>();
            services.AddSingleton<ILogic<MailingList>, BaseLogic<MailingList>>();
            services.AddSingleton<ILogic<MailingListSubscriber>, BaseLogic<MailingListSubscriber>>();
            services.AddSingleton<ILogic<MailingListSubscriberRelation>, BaseLogic<MailingListSubscriberRelation>>();
            services.AddSingleton<ILogic<Menu>, BaseLogic<Menu>>();
            services.AddSingleton<ILogic<Module>, BaseLogic<Module>>();
            services.AddSingleton<ILogic<OrderDetail>, BaseLogic<OrderDetail>>();
            services.AddSingleton<ILogic<Order>, BaseLogic<Order>>();
            services.AddSingleton<ILogic<Page>, BaseLogic<Page>>();
            services.AddSingleton<ILogic<PictureAlbum>, BaseLogic<PictureAlbum>>();
            services.AddSingleton<ILogic<Picture>, BaseLogic<Picture>>();
            services.AddSingleton<ILogic<Plugin>, BaseLogic<Plugin>>();
            services.AddSingleton<ILogic<PollAnswer>, BaseLogic<PollAnswer>>();
            services.AddSingleton<ILogic<Poll>, BaseLogic<Poll>>();
            services.AddSingleton<ILogic<PollUsersVote>, BaseLogic<PollUsersVote>>();
            services.AddSingleton<ILogic<PollVote>, BaseLogic<PollVote>>();
            services.AddSingleton<ILogic<ProductCategory>, BaseLogic<ProductCategory>>();
            services.AddSingleton<ILogic<Product>, BaseLogic<Product>>();
            services.AddSingleton<ILogic<ProductOption>, BaseLogic<ProductOption>>();
            services.AddSingleton<ILogic<Profile>, BaseLogic<Profile>>();
            services.AddSingleton<ILogic<Rss>, BaseLogic<Rss>>();
            services.AddSingleton<ILogic<ShoppingCart>, BaseLogic<ShoppingCart>>();
            services.AddSingleton<ILogic<SlideShow>, BaseLogic<SlideShow>>();
            services.AddSingleton<ILogic<VideoAlbum>, BaseLogic<VideoAlbum>>();
            services.AddSingleton<ILogic<Video>, BaseLogic<Video>>();
            services.AddSingleton<ILogic<VisitorInfo>, BaseLogic<VisitorInfo>>();
            services.AddSingleton<ILogic<VisitorSession>, BaseLogic<VisitorSession>>();
            services.AddSingleton<ILogic<Zone>, BaseLogic<Zone>>();
            services.AddSingleton<IChatLogic, ChatLogic>();

            services.AddSingleton<IConfigLogic, ConfigLogic>();
            services.AddSingleton<ICommentLogic, CommentLogic>();
        }
    }
}
