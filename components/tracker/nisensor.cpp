/****************************************************************************
*                                                                           *
****************************************************************************/

#include "nisensor.h"
#include "diagnostics.h"

namespace ImolaRetinaTracker
{

	void XN_CALLBACK_TYPE User_NewUser(xn::UserGenerator& generator, XnUserID nId, void* pCookie)
	{
		Diagnostics::Trace("New User %d\n", nId);

		if (NISensor::m_fUserNew != NULL)
		{
			NISensor::m_fUserNew();
		}
	}

	void XN_CALLBACK_TYPE User_LostUser(xn::UserGenerator& generator, XnUserID nId, void* pCookie)
	{
		Diagnostics::Trace("Lost User %d\n", nId);

		if (NISensor::m_fUserLost != NULL)
		{
			NISensor::m_fUserLost();
		}
	}

	void XN_CALLBACK_TYPE User_CalibrationStart(xn::SkeletonCapability& capability, XnUserID nId, void* pCookie)
	{
		if (NISensor::m_fUserCallibrationStart != NULL)
		{
			NISensor::m_fUserCallibrationStart();
		}
	}

	void XN_CALLBACK_TYPE User_CalibrationInProgress(xn::SkeletonCapability& capability, XnUserID id, XnCalibrationStatus calibrationError, void* pCookie)
	{
		if (NISensor::m_fUserCalibrationInProgress != NULL)
		{
			NISensor::m_fUserCalibrationInProgress();
		}
	}

	void XN_CALLBACK_TYPE User_CalibrationComplete(xn::SkeletonCapability& capability, XnUserID nId, XnCalibrationStatus eStatus, void* pCookie)
	{
		if (NISensor::m_fUserCallibrationComplete != NULL)
		{
			NISensor::m_fUserCallibrationComplete();
		}
	}

	void XN_CALLBACK_TYPE User_PoseDetected(xn::PoseDetectionCapability& capability, const XnChar* strPose, XnUserID nId, void* pCookie)
	{
		Diagnostics::Trace("Pose %s detected for user %d\n", strPose, nId);
		m_userGenerator.GetPoseDetectionCap().StopPoseDetection(nId);
		m_userGenerator.GetSkeletonCap().RequestCalibration(nId, TRUE);
		
		if (NISensor::m_fUserPoseDetected != NULL)
		{
			NISensor::m_fUserPoseDetected();
		}
	}

	void XN_CALLBACK_TYPE User_PoseInProgress(xn::PoseDetectionCapability& capability, const XnChar* strPose, XnUserID id, XnPoseDetectionStatus poseError, void* pCookie)
	{
		if (NISensor::m_fUserPoseInProgress != NULL)
		{
			NISensor::m_fUserPoseInProgress();
		}
	}

	NISensor::NISensor() 
	{
		m_bNeedPose = FALSE;

		m_hUserCallbacks = NULL;
		m_hCalibrationStart = NULL;
		m_hCalibrationComplete = NULL;
		m_hPoseDetected = NULL;
		m_hCalibrationInProgress = NULL;
		m_hPoseInProgress = NULL;
	}

	NISensor::~NISensor() {}
		
	NISensorStatus NISensor::Start()
	{
		XnStatus nRetVal = XN_STATUS_OK;
		xn::EnumerationErrors errors;
		m_context.InitFromXmlFile(NISensorConfiguration::sm_openNICfg, m_scriptNode, &errors);
		if (nRetVal == XN_STATUS_NO_NODE_PRESENT)
		{
			Diagnostics::Trace(errors);
			return NISENSOR_ERROR_UNKNOWN;
		}
		else if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Open failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}
	
		nRetVal = m_context.FindExistingNode(XN_NODE_TYPE_DEPTH, m_depthGenerator);
		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("No depth generator found ...");
			return NISENSOR_ERROR_UNKNOWN;
		}

		nRetVal = m_context.FindExistingNode(XN_NODE_TYPE_USER, m_userGenerator);
		if (nRetVal != XN_STATUS_OK)
		{
			nRetVal = m_userGenerator.Create(m_context);
			if (nRetVal != XN_STATUS_OK)
			{
				Diagnostics::Trace("Find user generator failed: %s\n", xnGetStatusString(nRetVal));
				return NISENSOR_ERROR_UNKNOWN;
			}
		}

		if (!m_userGenerator.IsCapabilitySupported(XN_CAPABILITY_SKELETON))
		{
			Diagnostics::Trace("Supplied user generator doesn't support skeleton\n");
			return NISENSOR_ERROR_UNKNOWN;
		}

		nRetVal = m_userGenerator.RegisterUserCallbacks(User_NewUser, User_LostUser, NULL, m_hUserCallbacks);

		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Register to user callbacks failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}


		nRetVal = m_userGenerator.GetSkeletonCap().RegisterToCalibrationStart(User_CalibrationStart, NULL, m_hCalibrationStart);
		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Register to calibration start failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}

			
		nRetVal = m_userGenerator.GetSkeletonCap().RegisterToCalibrationComplete(User_CalibrationComplete, NULL, m_hCalibrationComplete);

		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Register to calibration complete failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}

		switch(NISensorConfiguration::sm_skeletonProfile)
		{
			case NISENSOR_SKELETON_PROFILE_ALL:
				m_userGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_ALL);
				break;
			case NISENSOR_SKELETON_PROFILE_HEAD_HANDS:
				m_userGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_HEAD_HANDS);
				break;
			case NISENSOR_SKELETON_PROFILE_LOWER:
				m_userGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_LOWER);
				break;
			case NISENSOR_SKELETON_PROFILE_NONE:
				m_userGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_NONE);
				break;
			case NISENSOR_SKELETON_PROFILE_UPPER:
				m_userGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_UPPER);
				break;
			default:
				Diagnostics::Trace("Unknow skeleton type configuration \n");
				return NISENSOR_ERROR_UNKNOWN;
		}
	
		nRetVal = m_userGenerator.GetSkeletonCap().RegisterToCalibrationInProgress(User_CalibrationInProgress, NULL, m_hCalibrationInProgress);
		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Register to calibration in progress failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}

		nRetVal = m_userGenerator.GetPoseDetectionCap().RegisterToPoseInProgress(User_PoseInProgress, NULL, m_hPoseInProgress);
		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Register to pose in progress failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}

		nRetVal = m_context.StartGeneratingAll();
		if (nRetVal != XN_STATUS_OK)
		{
			Diagnostics::Trace("Generating start failed: %s\n", xnGetStatusString(nRetVal));
			return NISENSOR_ERROR_UNKNOWN;
		}

		return NISENSOR_STATUS_OK;
	}

	NISensorStatus NISensor::Stop() {}
}