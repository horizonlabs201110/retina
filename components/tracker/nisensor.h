/****************************************************************************
*                                                                           *
****************************************************************************/

#ifndef __NISENSOR_H__
#define __NISENSOR_H__

#include <XnOpenNI.h>
#include <XnCodecIDs.h>
#include <XnCppWrapper.h>
#include <XnPropNames.h>

namespace ImolaRetinaTracker
{
	typedef void (*Func_User_New)();
	typedef void (*Func_User_Lost)();
	typedef void (*Func_User_CalibrationStart)();
	typedef void (*Func_User_CalibrationInProgress)();
	typedef void (*Func_User_CalibrationComplete)();
	typedef void (*Func_User_PoseDetected)();
	typedef void (*Func_User_PoseInProgress)();
	
	typedef enum NISensorStatus	
	{
		NISENSOR_STATUS_OK = 0,
		NISENSOR_ERROR_UNKNOWN = 1
	} NISensorStatus;

	typedef enum NISensorSkeletonProfile
	{
		NISENSOR_SKELETON_PROFILE_NONE		= 1,
		NISENSOR_SKELETON_PROFILE_ALL			= 2,
		NISENSOR_SKELETON_PROFILE_UPPER		= 3,
		NISENSOR_SKELETON_PROFILE_LOWER		= 4,
		NISENSOR_SKELETON_PROFILE_HEAD_HANDS	= 5,
	} NISensorSkeletonProfile;

	class NISensor
	{
	private:
		xn::Context m_context;
		xn::ScriptNode m_scriptNode;
		xn::DepthGenerator m_depthGenerator;
		xn::UserGenerator m_userGenerator;
		xn::Player m_player;

		XnCallbackHandle m_hUserCallbacks;
		XnCallbackHandle m_hCalibrationStart;
		XnCallbackHandle m_hCalibrationComplete;
		XnCallbackHandle m_hPoseDetected;
		XnCallbackHandle m_hCalibrationInProgress;
		XnCallbackHandle m_hPoseInProgress;

		XnBool m_bNeedPose;
		XnChar m_strPose[20];

	public:
		static Func_User_New m_fUserNew;
		static Func_User_Lost m_fUserLost;
		static Func_User_CalibrationStart m_fUserCallibrationStart;
		static Func_User_CalibrationInProgress m_fUserCalibrationInProgress;
		static Func_User_CalibrationComplete m_fUserCallibrationComplete;
		static Func_User_PoseDetected m_fUserPoseDetected;
		static Func_User_PoseInProgress m_fUserPoseInProgress;

		NISensor();
		~NISensor();
		NISensorStatus Start();
		NISensorStatus Stop();
	};

	class NISensorConfiguration
	{
	public:
		static const char* sm_openNICfg;
		static const NISensorSkeletonProfile sm_skeletonProfile;
	};
}

#endif