# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator.
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class User(Model):
    """User.

    :param tenant:
    :type tenant: str
    :param user_type: Possible values include: 'User', 'System', 'Provider',
     'Application'
    :type user_type: str or ~teamcloud.models.enum
    :param role: Possible values include: 'None', 'Creator', 'Admin'
    :type role: str or ~teamcloud.models.enum
    :param project_memberships:
    :type project_memberships: list[~teamcloud.models.ProjectMembership]
    :param properties:
    :type properties: dict[str, str]
    :param id:
    :type id: str
    """

    _attribute_map = {
        'tenant': {'key': 'tenant', 'type': 'str'},
        'user_type': {'key': 'userType', 'type': 'str'},
        'role': {'key': 'role', 'type': 'str'},
        'project_memberships': {'key': 'projectMemberships', 'type': '[ProjectMembership]'},
        'properties': {'key': 'properties', 'type': '{str}'},
        'id': {'key': 'id', 'type': 'str'},
    }

    def __init__(self, **kwargs):
        super(User, self).__init__(**kwargs)
        self.tenant = kwargs.get('tenant', None)
        self.user_type = kwargs.get('user_type', None)
        self.role = kwargs.get('role', None)
        self.project_memberships = kwargs.get('project_memberships', None)
        self.properties = kwargs.get('properties', None)
        self.id = kwargs.get('id', None)
