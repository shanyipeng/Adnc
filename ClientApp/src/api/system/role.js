import request from '@/utils/request'

export function getList(params) {
  return request({
    url: '/sys/roles',
    method: 'get',
    params
  })
}


export function save(data) {
  return request({
    url: '/sys/roles',
    method: 'post',
    data
  })
}

export function remove(roleId) {
  return request({
    url: `/sys/roles/${roleId}`,
    method: 'delete'
  })
}

export  function roleTreeListByUserId(userId){
  return request({
    url: `/sys/roles/${userId}/rolestree`,
    method: 'get'
  })
}


export function changePermissons(roleId, permissons) {
  return request({
    url: `/sys/roles/${roleId}/permissons`,
    method: 'put',
    data: permissons
  })
}
