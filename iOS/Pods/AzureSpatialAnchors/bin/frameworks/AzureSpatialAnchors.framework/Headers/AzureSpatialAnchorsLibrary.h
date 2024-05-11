// AzureSpatialAnchors
// This file was auto-generated from SscApiModelDirect.cs.

#import <Foundation/Foundation.h>
#import <ARKit/ARKit.h>

// Enumerations.
/// Defines logging severity levels.
typedef NS_ENUM(NSInteger, ASASessionLogLevel)
{
    /// Specifies that logging should not write any messages.
    ASASessionLogLevelNone = 0,
    /// Specifies logs that indicate when the current flow of execution stops due to a failure.
    ASASessionLogLevelError = 1,
    /// Specifies logs that highlight an abnormal or unexpected event, but do not otherwise cause execution to stop.
    ASASessionLogLevelWarning = 2,
    /// Specifies logs that track the general flow.
    ASASessionLogLevelInformation = 3,
    /// Specifies logs used for interactive investigation during development.
    ASASessionLogLevelDebug = 4,
    /// Specifies all messages should be logged.
    ASASessionLogLevelAll = 5
};

/// Use this enumeration to determine whether an anchor was located, and the reason why it may have failed.
typedef NS_ENUM(NSInteger, ASALocateAnchorStatus)
{
    /// The anchor was already being tracked.
    ASALocateAnchorStatusAlreadyTracked = 0,
    /// The anchor was found.
    ASALocateAnchorStatusLocated = 1,
    /// The anchor was not found.
    ASALocateAnchorStatusNotLocated = 2,
    /// The anchor cannot be found - it was deleted or the identifier queried for was incorrect.
    ASALocateAnchorStatusNotLocatedAnchorDoesNotExist = 3
};

/// Use this enumeration to indicate the method by which anchors can be located.
typedef NS_ENUM(NSInteger, ASALocateStrategy)
{
    /// Indicates that any method is acceptable.
    ASALocateStrategyAnyStrategy = 0,
    /// Indicates that anchors will be located only by visual information.
    ASALocateStrategyVisualInformation = 1,
    /// Indicates that anchors will be located primarily by relationship to other anchors.
    ASALocateStrategyRelationship = 2
};

/// Possible values returned when querying PlatformLocationProvider for GeoLocation capabilities
typedef NS_ENUM(NSInteger, ASAGeoLocationStatusResult)
{
    /// GeoLocation data is available.
    ASAGeoLocationStatusResultAvailable = 0,
    /// GeoLocation was disabled in the SensorCapabilities.
    ASAGeoLocationStatusResultDisabledCapability = 1,
    /// No sensor fingerprint provider has been created.
    ASAGeoLocationStatusResultMissingSensorFingerprintProvider = 2,
    /// No GPS data has been received.
    ASAGeoLocationStatusResultNoGPSData = 3
};

/// Possible values returned when querying PlatformLocationProvider for Wifi capabilities
typedef NS_ENUM(NSInteger, ASAWifiStatusResult)
{
    /// Wifi data is available.
    ASAWifiStatusResultAvailable = 0,
    /// Wifi was disabled in the SensorCapabilities.
    ASAWifiStatusResultDisabledCapability = 1,
    /// No sensor fingerprint provider has been created.
    ASAWifiStatusResultMissingSensorFingerprintProvider = 2,
    /// No Wifi access points have been found.
    ASAWifiStatusResultNoAccessPointsFound = 3
};

/// Possible values returned when querying PlatformLocationProvider for Bluetooth capabilities
typedef NS_ENUM(NSInteger, ASABluetoothStatusResult)
{
    /// Bluetooth beacons data is available.
    ASABluetoothStatusResultAvailable = 0,
    /// Bluetooth was disabled in the SensorCapabilities.
    ASABluetoothStatusResultDisabledCapability = 1,
    /// No sensor fingerprint provider has been created.
    ASABluetoothStatusResultMissingSensorFingerprintProvider = 2,
    /// No bluetooth beacons have been found.
    ASABluetoothStatusResultNoBeaconsFound = 3
};

/// Use this enumeration to describe the kind of feedback that can be provided to the user about data
typedef NS_OPTIONS(NSInteger, ASASessionUserFeedback)
{
    /// No specific feedback is available.
    ASASessionUserFeedbackNone = 0,
    /// Device is not moving enough to create a neighborhood of key-frames.
    ASASessionUserFeedbackNotEnoughMotion = 1,
    /// Device is moving too quickly for stable tracking.
    ASASessionUserFeedbackMotionTooQuick = 2,
    /// The environment doesn't have enough feature points for stable tracking.
    ASASessionUserFeedbackNotEnoughFeatures = 4
};

/// Identifies the source of an error in a cloud spatial session.
typedef NS_ENUM(NSInteger, ASACloudSpatialErrorCode)
{
    /// Amount of Metadata exceeded the allowed limit (currently 4k).
    ASACloudSpatialErrorCodeMetadataTooLarge = 0,
    /// Application did not provide valid credentials and therefore could not authenticate with the Cloud Service.
    ASACloudSpatialErrorCodeApplicationNotAuthenticated = 1,
    /// Application did not provide any credentials for authorization with the Cloud Service.
    ASACloudSpatialErrorCodeApplicationNotAuthorized = 2,
    /// Multiple stores (on the same device or different devices) made concurrent changes to the same Spatial Entity and so this particular change was rejected.
    ASACloudSpatialErrorCodeConcurrencyViolation = 3,
    /// Not enough Neighborhood Spatial Data was available to complete the desired Create operation.
    ASACloudSpatialErrorCodeNotEnoughSpatialData = 4,
    /// No Spatial Location Hint was available (or it was not specific enough) to support rediscovery from the Cloud at a later time.
    ASACloudSpatialErrorCodeNoSpatialLocationHint = 5,
    /// Application cannot connect to the Cloud Service.
    ASACloudSpatialErrorCodeCannotConnectToServer = 6,
    /// Cloud Service returned an unspecified error.
    ASACloudSpatialErrorCodeServerError = 7,
    /// The Spatial Entity has already been associated with a different Store object, so cannot be used with this current Store object.
    ASACloudSpatialErrorCodeAlreadyAssociatedWithADifferentStore = 8,
    /// SpatialEntity already exists in a Store but TryCreateAsync was called.
    ASACloudSpatialErrorCodeAlreadyExists = 9,
    /// A locate operation was requested, but the criteria does not specify anything to look for.
    ASACloudSpatialErrorCodeNoLocateCriteriaSpecified = 10,
    /// An access token was required but not specified; handle the TokenRequired event on the session to provide one.
    ASACloudSpatialErrorCodeNoAccessTokenSpecified = 11,
    /// The session was unable to obtain an access token and so the creation could not proceed.
    ASACloudSpatialErrorCodeUnableToObtainAccessToken = 12,
    /// There were too many requests made from this Account ID, so it is being throttled.
    ASACloudSpatialErrorCodeTooManyRequests = 13,
    /// The LocateCriteria options that were specified are not valid because they're missing a required value.
    ASACloudSpatialErrorCodeLocateCriteriaMissingRequiredValues = 14,
    /// The LocateCriteria options that were specified are not valid because they're in conflict with settings for another mode.
    ASACloudSpatialErrorCodeLocateCriteriaInConflict = 15,
    /// The LocateCriteria options that were specified are not valid values.
    ASACloudSpatialErrorCodeLocateCriteriaInvalid = 16,
    /// The LocateCriteria options that were specified are not valid because they're not currently supported.
    ASACloudSpatialErrorCodeLocateCriteriaNotSupported = 17,
    /// Encountered an unknown error on the session.
    ASACloudSpatialErrorCodeUnknown = 19,
    /// The Http request timed out.
    ASACloudSpatialErrorCodeHttpTimeout = 20,
    /// The transform matrix of platform anchor passed in is invalid. It is either not affine, or its rotation matrix is not orthonormal. The second case may be caused due to its containing a non-uniform scale or a uniform scale other than 1, or just some malformed values.
    ASACloudSpatialErrorCodeInvalidAnchorTransformRigidity = 21
};

/// Use the data category values to determine what data is returned in an AnchorLocateCriteria object.
typedef NS_OPTIONS(NSInteger, ASAAnchorDataCategory)
{
    /// No data is returned.
    ASAAnchorDataCategoryNone = 0,
    /// Returns Anchor properties including AppProperties.
    ASAAnchorDataCategoryProperties = 1,
    /// Returns spatial information about an Anchor.
    ASAAnchorDataCategorySpatial = 2
};

// MARK: Forward declarations.
@class ASALocateAnchorsCompletedEventArgs;
@class ASACloudSpatialAnchorWatcher;
@class ASAAnchorLocatedEventArgs;
@class ASACloudSpatialAnchor;
@class ASASessionConfiguration;
@class ASASensorCapabilities;
@class ASAPlatformLocationProvider;
@class ASAGeoLocation;
@class ASACloudSpatialAnchorSession;
@class ASACloudSpatialAnchorSessionDiagnostics;
@class ASATokenRequiredEventArgs;
@class ASACloudSpatialAnchorSessionDeferral;
@class ASASessionUpdatedEventArgs;
@class ASASessionStatus;
@class ASASessionErrorEventArgs;
@class ASAOnLogDebugEventArgs;
@class ASASensorFingerprintEventArgs;
@class ASAAnchorLocateCriteria;
@class ASANearAnchorCriteria;
@class ASANearDeviceCriteria;
@protocol ASACloudSpatialAnchorSessionDelegate;

/**
 * A set of methods that are called by ASACloudSpatialAnchorSession in response to important events.
 */
@protocol ASACloudSpatialAnchorSessionDelegate <NSObject>
@optional
- (void)tokenRequired:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASATokenRequiredEventArgs *)args;
- (void)anchorLocated:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASAAnchorLocatedEventArgs *)args;
- (void)locateAnchorsCompleted:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASALocateAnchorsCompletedEventArgs *)args;
- (void)sessionUpdated:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASASessionUpdatedEventArgs *)args;
- (void)error:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASASessionErrorEventArgs *)args;
- (void)onLogDebug:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASAOnLogDebugEventArgs *)args;
- (void)updatedSensorFingerprintRequired:(ASACloudSpatialAnchorSession *)cloudSpatialAnchorSession :(ASASensorFingerprintEventArgs *)args;
@end

/// Use this type to determine when a locate operation has completed.
@interface ASALocateAnchorsCompletedEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Gets a value indicating whether the locate operation was canceled.
@property (readonly) BOOL cancelled;

/// The watcher that completed the locate operation.
@property (retain, readonly) ASACloudSpatialAnchorWatcher * watcher;

@end

/// Use this class to control an object that watches for spatial anchors.
@interface ASACloudSpatialAnchorWatcher : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Distinct identifier for the watcher within its session.
@property (readonly) int identifier;

/// Stops further activity from this watcher.
-(void)stop;

@end

/// Use this type to determine the status of an anchor after a locate operation.
@interface ASAAnchorLocatedEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The cloud spatial anchor that was located.
@property (retain, readonly) ASACloudSpatialAnchor * anchor;

/// The spatial anchor that was located.
@property (retain, readonly) NSString * identifier;

/// Specifies whether the anchor was located, or the reason why it may have failed.
@property (readonly) ASALocateAnchorStatus status;

/// Gets the LocateStrategy that reflects the strategy that was used to find the anchor. Valid only when the anchor was found.
@property (readonly) ASALocateStrategy strategy;

/// The watcher that located the anchor.
@property (retain, readonly) ASACloudSpatialAnchorWatcher * watcher;

@end

/// Use this class to represent an anchor in space that can be persisted in a CloudSpatialAnchorSession.
@interface ASACloudSpatialAnchor : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The anchor in the local mixed reality system.
@property (retain) ARAnchor * localAnchor;

/// The time the anchor will expire.
@property (copy) NSDate * expiration;

/// The persistent identifier of this spatial anchor in the cloud service.
@property (retain, readonly) NSString * identifier;

/// A dictionary of application-defined properties that is stored with the anchor.
@property (copy) NSDictionary * appProperties;

/// An opaque version tag that can be used for concurrency control.
@property (retain, readonly) NSString * versionTag;

@end

/// Use this class to set up the service configuration for a SpatialAnchorSession.
@interface ASASessionConfiguration : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Account domain for the Azure Spatial Anchors service.
@property (retain) NSString * accountDomain;

/// Account-level ID for the Azure Spatial Anchors service.
@property (retain) NSString * accountId;

/// Authentication token for Azure Active Directory (AAD).
@property (retain) NSString * authenticationToken;

/// Account-level key for the Azure Spatial Anchors service.
@property (retain) NSString * accountKey;

/// Access token for the Azure Spatial Anchors service.
@property (retain) NSString * accessToken;

@end

/// Use this class to give the session access to sensors to help find anchors around you. This is typically used by a [PlatformLocationProvider](./platformlocationprovider.md) to configure sensors. Refer to [Coarse Relocalization](https://aka.ms/CoarseRelocalization) to learn more about sensors and platforms.
@interface ASASensorCapabilities : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Whether to use the device's global position to find anchors and improve the locatability of existing anchors.
@property BOOL geoLocationEnabled;

/// Whether to use WiFi signals to find anchors and improve the locatability of existing anchors.
@property BOOL wifiEnabled;

/// Whether to use Bluetooth signals to find anchors and improve the locatability of existing anchors. Make sure that you also set KnownBeaconProximityUuids when enabling Bluetooth.
@property BOOL bluetoothEnabled;

/// Controls which Bluetooth beacon devices the session is able to see. Add the proximity UUIDs here for all beacons you want to use to find anchors and improve the locatability of existing anchors.
@property (copy) NSArray<NSString *> * knownBeaconProximityUuids;

@end

/// Use this class to get an estimate of the current location of your device. A PlatformLocationProvider is typically passed to a [CloudSpatialAnchorSession](./cloudspatialanchorsession.md) to enable large-scale relocalization scenario with [Coarse Relocalization](https://aka.ms/CoarseRelocalization).
@interface ASAPlatformLocationProvider : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The sensors used by the session to locate anchors around you and annotate created anchors so that they can be found.
@property (retain, readonly) ASASensorCapabilities * sensors;

/// Checks whether sufficient sensor data is available to locate or create anchors tagged with geolocation.
@property (readonly) ASAGeoLocationStatusResult geoLocationStatus;

/// Checks whether sufficient sensor data is available to locate or create anchors tagged with Wi-Fi signals.
@property (readonly) ASAWifiStatusResult wifiStatus;

/// Checks whether sufficient sensor data is available to locate or create anchors tagged with Bluetooth signals.
@property (readonly) ASABluetoothStatusResult bluetoothStatus;

/// Returns the latest estimate of the device's location.
-(ASAGeoLocation *)getLocationEstimate;

/// Start tracking the device's location.
-(void)start;

/// Stop tracking the device's location.
-(void)stop;

@end

/// Contains optional geographical location information within a sensor fingerprint.
@interface ASAGeoLocation : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The current latitude of the device in degrees.
@property double latitude;

/// The current longitude of the device in degrees.
@property double longitude;

/// The horizontal error in meters of the latitude and longitude. This corresponds to the radius of a 68.3% confidence region on the East/North plane. Over many invocations, the true position should be within this number of horizontal meters of the reported position.
@property float horizontalError;

/// The altitude of the device in meters.
@property float altitude;

/// The vertical error of the altitude in meters. This corresponds to a one-sided 68.3% confidence interval along the Up axis. Over many invocations, the true altitude should be within this number of meters of the reported altitude.
@property float verticalError;

@end

/// Use this class to create, locate and manage spatial anchors.
@interface ASACloudSpatialAnchorSession : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The configuration information for the session.
@property (retain, readonly) ASASessionConfiguration * configuration;

/// The diagnostics settings for the session, which can be used to collect and submit data for troubleshooting and improvements.
@property (retain, readonly) ASACloudSpatialAnchorSessionDiagnostics * diagnostics;

/// Logging level for the session log events.
@property ASASessionLogLevel logLevel;

/// The tracking session used to help locate anchors.
@property (retain) ARSession * session;

@property BOOL telemetryEnabled;

/// Location provider used to create and locate anchors using [Coarse Relocalization](https://aka.ms/CoarseRelocalization).
@property (retain) ASAPlatformLocationProvider * locationProvider;

/// The unique identifier for the session.
@property (retain, readonly) NSString * sessionId;

/**
 * The delegate that will handle events from the ASACloudSpatialAnchorSession.
 */
@property(nonatomic, assign) id<ASACloudSpatialAnchorSessionDelegate> delegate;

/// Stops this session and releases all associated resources.
-(void)dispose;

/// Gets the Azure Spatial Anchors access token from authentication token.
-(void)getAccessTokenWithAuthenticationToken:(NSString *)authenticationToken withCompletionHandler:(void (^)(NSString * value, NSError *error))completionHandler;

/// Gets the Azure Spatial Anchors access token from account key.
-(void)getAccessTokenWithAccountKey:(NSString *)accountKey withCompletionHandler:(void (^)(NSString * value, NSError *error))completionHandler;

/// Creates a new persisted spatial anchor from the specified local anchor and string properties.
-(void)createAnchor:(ASACloudSpatialAnchor *)anchor withCompletionHandler:(void (^)(NSError *error))completionHandler;

/// Creates a new object that watches for anchors that meet the specified criteria.
-(ASACloudSpatialAnchorWatcher *)createWatcher:(ASAAnchorLocateCriteria *)criteria;

/// Gets a cloud spatial anchor for the given identifier, even if it hasn't been located yet.
-(void)getAnchorProperties:(NSString *)identifier withCompletionHandler:(void (^)(ASACloudSpatialAnchor * value, NSError *error))completionHandler;

/// Gets a list of all nearby cloud spatial anchor ids corresponding to a given criteria.
-(void)getNearbyAnchorIds:(ASANearDeviceCriteria *)criteria withCompletionHandler:(void (^)(NSArray * value, NSError *error))completionHandler;

/// Gets a list of active watchers.
-(NSArray<ASACloudSpatialAnchorWatcher *> *)getActiveWatchers;

/// Refreshes properties for the specified spatial anchor.
-(void)refreshAnchorProperties:(ASACloudSpatialAnchor *)anchor withCompletionHandler:(void (^)(NSError *error))completionHandler;

/// Updates the specified spatial anchor.
-(void)updateAnchorProperties:(ASACloudSpatialAnchor *)anchor withCompletionHandler:(void (^)(NSError *error))completionHandler;

/// Deletes a persisted spatial anchor.
-(void)deleteAnchor:(ASACloudSpatialAnchor *)anchor withCompletionHandler:(void (^)(NSError *error))completionHandler;

/// Applications must call this method on platforms where per-frame processing is required.
-(void)processFrame:(ARFrame *)frame;

/// Gets an object describing the status of the session.
-(void)getSessionStatusWithCompletionHandler:(void (^)(ASASessionStatus * value, NSError *error))completionHandler;

/// Begins capturing environment data for the session.
-(void)start;

/// Stops capturing environment data for the session and cancels any outstanding locate operations. Environment data is maintained.
-(void)stop;

/// Resets environment data that has been captured in this session; applications must call this method when tracking is lost.
-(void)reset;

@end

/// Use this class to configure session diagnostics that can be collected and submitted to improve system quality.
@interface ASACloudSpatialAnchorSessionDiagnostics : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Level of tracing to log.
@property ASASessionLogLevel logLevel;

/// Directory into which temporary log files and manifests are saved.
@property (retain) NSString * logDirectory;

/// Approximate maximum disk space to be used, in megabytes.
@property int maxDiskSizeInMB;

/// Whether images should be logged.
@property BOOL imagesEnabled;

/// Creates a manifest of log files and submission information to be uploaded.
-(void)createManifest:(NSString *)description withCompletionHandler:(void (^)(NSString * value, NSError *error))completionHandler;

/// Submits a diagnostics manifest and cleans up its resources.
-(void)submitManifest:(NSString *)manifestPath withCompletionHandler:(void (^)(NSError *error))completionHandler;

@end

/// Informs the application that the service requires an updated access token or authentication token.
@interface ASATokenRequiredEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The access token to be used by the operation that requires it.
@property (retain) NSString * accessToken;

/// The authentication token to be used by the operation that requires it.
@property (retain) NSString * authenticationToken;

/// Returns a deferral object that can be used to provide an updated access token or authentication token from another asynchronous operation.
-(ASACloudSpatialAnchorSessionDeferral *)getDeferral;

@end

/// Use this class to defer completing an operation.
@interface ASACloudSpatialAnchorSessionDeferral : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Mark the deferred operation as complete and perform any associated tasks.
-(void)complete;

@end

/// Provides data for the event that fires when the session state is updated.
@interface ASASessionUpdatedEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Current session status.
@property (retain, readonly) ASASessionStatus * status;

@end

/// This type describes the status of spatial data processing.
@interface ASASessionStatus : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The level of data sufficiency for a successful operation.
@property (readonly) float readyForCreateProgress;

/// The ratio of data available to recommended data to create an anchor.
@property (readonly) float recommendedForCreateProgress;

/// A hash value that can be used to know when environment data that contributes to a creation operation has changed to included the latest input data.
@property (readonly) int sessionCreateHash;

/// A hash value that can be used to know when environment data that contributes to a locate operation has changed to included the latest input data.
@property (readonly) int sessionLocateHash;

/// Feedback that can be provided to user about data processing status.
@property (readonly) ASASessionUserFeedback userFeedback;

@end

/// Provides data for the event that fires when errors are thrown.
@interface ASASessionErrorEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The error code.
@property (readonly) ASACloudSpatialErrorCode errorCode;

/// The error message.
@property (retain, readonly) NSString * errorMessage;

/// The watcher that found an error, possibly null.
@property (retain, readonly) ASACloudSpatialAnchorWatcher * watcher;

@end

/// Provides data for the event that fires for logging messages.
@interface ASAOnLogDebugEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The logging message.
@property (retain, readonly) NSString * message;

@end

/// Informs the application that the service would like an updated sensor fingerprint.
@interface ASASensorFingerprintEventArgs : NSObject
/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// The current geographical position of the device if available.
@property (retain) ASAGeoLocation * geoPosition;

@end

/// Specifies a set of criteria for locating anchors.
@interface ASAAnchorLocateCriteria : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Whether locate should bypass the local cache of anchors.
@property BOOL bypassCache;

/// Categories of data that are requested.
@property ASAAnchorDataCategory requestedCategories;

/// Indicates the strategy by which anchors will be located.
@property ASALocateStrategy strategy;

/// Indicates the CloudSpatialAnchor identifiers to locate. Maximum limit of 35 anchors per watcher.
@property (copy) NSArray<NSString *> * identifiers;

/// Indicates that anchors to locate should be close to a specific anchor.
@property (retain) ASANearAnchorCriteria * nearAnchor;

/// Indicates that anchors to locate should be close to the device.
@property (retain) ASANearDeviceCriteria * nearDevice;

@end

/// Use this class to describe how anchors to be located should be near a source anchor.
@interface ASANearAnchorCriteria : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Source anchor around which nearby anchors should be located.
@property (retain) ASACloudSpatialAnchor * sourceAnchor;

/// Maximum distance in meters from the source anchor (defaults to 5).
@property float distanceInMeters;

/// Maximum desired result count (defaults to 20).
@property int maxResultCount;

@end

/// Use this class to describe how anchors to be located should be near the device.
@interface ASANearDeviceCriteria : NSObject
-(instancetype)init;

/// Deallocates the memory occupied by this object.
-(void)dealloc;

/// Maximum distance in meters from the device (defaults to 5).
@property float distanceInMeters;

/// Maximum desired result count (defaults to 20).
@property int maxResultCount;

@end

