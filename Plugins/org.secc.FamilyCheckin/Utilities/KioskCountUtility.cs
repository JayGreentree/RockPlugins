// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock;
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.CheckIn;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;

namespace org.secc.FamilyCheckin.Utilities
{
    public class KioskCountUtility
    {
        public List<int> ConfiguredGroupTypes { get; private set; }
        private List<GroupType> _groupTypes;
        public List<GroupType> GroupTypes
        {
            get
            {
                if ( _groupTypes == null )
                {
                    RockContext rockContext = new RockContext();

                    IQueryable<GroupType> groupTypeService = new GroupTypeService( rockContext ).Queryable( "Groups,Groups.GroupLocations,Groups.GroupLocations.Schedules" );

                    _groupTypes = groupTypeService
                        .Where( gt => ConfiguredGroupTypes.Contains( gt.Id ) ).ToList();
                }
                return _groupTypes;
            }
        }
        public List<int> VolunteerGroupIds { get => GetVolunteerGroupIds(); }
        public static List<int> GetVolunteerGroupIds()
        {
            if ( RockCache.Get( Constants.VOLUNTEER_ATTRIBUTE_GUID + "CachedVolunteerGroups" ) is List<int> volunteerGroupIds )
            {
                return volunteerGroupIds;
            }

            //get value from db and update
            var volAttribute = AttributeCache.Get( Constants.VOLUNTEER_ATTRIBUTE_GUID.AsGuid() );
            AttributeValueService attributeValueService = new AttributeValueService( new RockContext() );
            volunteerGroupIds = attributeValueService.Queryable().Where( av => av.AttributeId == volAttribute.Id && av.Value == "True" ).Select( av => av.EntityId.Value ).ToList();
            RockCache.AddOrUpdate( Constants.VOLUNTEER_ATTRIBUTE_GUID + "CachedVolunteerGroups", null, volunteerGroupIds, RockDateTime.Now.AddMinutes( 10 ), Constants.CACHE_TAG );

            return volunteerGroupIds;

        }

        public List<int> ChildGroupIds
        {
            get => GetChildGroupIds();
        }
        public static List<int> GetChildGroupIds()
        {
            if ( RockCache.Get( Constants.VOLUNTEER_ATTRIBUTE_GUID + "CachedChildGroups" ) is List<int> childGroupIds )
            {
                return childGroupIds;
            }

            //get value from db and update
            var volAttribute = AttributeCache.Get( Constants.VOLUNTEER_ATTRIBUTE_GUID.AsGuid() );
            AttributeValueService attributeValueService = new AttributeValueService( new RockContext() );
            childGroupIds = attributeValueService.Queryable().Where( av => av.AttributeId == volAttribute.Id && av.Value == "False" ).Select( av => av.EntityId.Value ).ToList();
            RockCache.AddOrUpdate( Constants.VOLUNTEER_ATTRIBUTE_GUID + "CachedChildGroups", null, childGroupIds, RockDateTime.Now.AddMinutes( 10 ), Constants.CACHE_TAG );

            return childGroupIds;
        }

        private Guid? _deactivatedDefinedTypeGuid;

        private List<GroupLocationSchedule> _groupLocationSchedules;
        public List<GroupLocationSchedule> GroupLocationSchedules
        {
            get
            {
                if ( _groupLocationSchedules == null )
                {
                    var groupLocations = GroupTypes
                        .SelectMany( gt => gt.Groups.Where( g => g.IsActive ) )
                        .SelectMany( g => g.GroupLocations );

                    _groupLocationSchedules = new List<GroupLocationSchedule>();

                    foreach ( var gl in groupLocations )
                    {
                        foreach ( var s in gl.Schedules )
                        {
                            _groupLocationSchedules.Add( new GroupLocationSchedule( gl, s, true ) );
                        }
                    }

                    if ( _deactivatedDefinedTypeGuid != null )
                    {
                        RockContext rockContext = new RockContext();
                        GroupLocationService groupLocationService = new GroupLocationService( rockContext );
                        ScheduleService scheduleService = new ScheduleService( rockContext );

                        var dtDeactivated = DefinedTypeCache.Get( _deactivatedDefinedTypeGuid ?? new Guid() );
                        var dvDeactivated = dtDeactivated.DefinedValues;
                        _groupLocationSchedules.AddRange( dvDeactivated.Select( dv => dv.Value.Split( '|' ) )
                            .Select( s => new GroupLocationSchedule(
                                groupLocationService.Get( s[0].AsInteger() ),
                                scheduleService.Get( s[1].AsInteger() ),
                                false )
                            ).ToList()
                        );
                    }

                }
                return _groupLocationSchedules;
            }
        }

        public KioskCountUtility( List<int> _configuredGroupTypes )
        {
            _KioskCountUtility( _configuredGroupTypes, null );
        }

        public KioskCountUtility( List<int> _configuredGroupTypes, Guid DeactivatedDefinedTypeGuid )
        {
            _KioskCountUtility( _configuredGroupTypes, DeactivatedDefinedTypeGuid );
        }

        private void _KioskCountUtility( List<int> _configuredGroupTypes, Guid? DeactivatedDefinedTypeGuid )
        {

            ConfiguredGroupTypes = _configuredGroupTypes;
            _deactivatedDefinedTypeGuid = DeactivatedDefinedTypeGuid;

        }

        public LocationScheduleCount GetLocationScheduleCount( int LocationId, int ScheduleId )
        {
            var locationScheduleCount = new LocationScheduleCount();
            var lglsc = CheckInCountCache.GetByLocation( LocationId ).Where( glsc => glsc.ScheduleId == ScheduleId ).ToList();

            locationScheduleCount.ChildCount = lglsc.Where( glsc => ChildGroupIds.Contains( glsc.GroupId ) )
                .Sum( glsc => glsc.PersonIds.Count );

            locationScheduleCount.VolunteerCount = lglsc.Where( glsc => VolunteerGroupIds.Contains( glsc.GroupId ) )
                .Sum( glsc => glsc.PersonIds.Count );

            locationScheduleCount.ReservedCount = lglsc.Sum( glsc => glsc.PersonIds.Count ) - lglsc.Sum( glsc => glsc.InRoomPersonIds.Count );

            locationScheduleCount.TotalCount = lglsc.Sum( glsc => glsc.PersonIds.Count );

            return locationScheduleCount;
        }
    }

    public class GroupLocationSchedule
    {
        public GroupLocation GroupLocation { get; private set; }
        public Schedule Schedule { get; private set; }
        public bool Active { get; private set; }

        internal GroupLocationSchedule( GroupLocation GroupLocation, Schedule Schedule, bool Active )
        {
            this.GroupLocation = GroupLocation;
            this.Schedule = Schedule;
            this.Active = Active;
        }
    }

    public class LocationScheduleCount
    {
        public int ChildCount { get; internal set; }
        public int VolunteerCount { get; internal set; }
        public int ReservedCount { get; internal set; }
        public int TotalCount { get; internal set; }


    }
}
